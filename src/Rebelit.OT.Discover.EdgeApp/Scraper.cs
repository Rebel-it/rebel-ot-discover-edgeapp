using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;

namespace Rebelit.OT.Discover.EdgeApp;

public class Scraper(
    IUAClientFactory clientFactory,
    IClientSamplerFactory clientSamplerFactory,
    IApiClient apiClient,
    IConfiguration configuration,
    ILogger<Scraper> logger
) : IScraper
{
    public string Address { get; } =
        configuration["OPCUA_ServerAddress"]
        ?? throw new InvalidOperationException(
            "OPCUA_ServerAddress configuration is not set."
        );

    public string AgentId { get; } =
        configuration["IXON_AgentId"]
        ?? throw new InvalidOperationException("IXON_AgentId configuration is not set.");

    public string Username { get; } =
        configuration["OPCUA_Username"]
        ?? throw new InvalidOperationException("OPCUA_Username configuration is not set.");

    public string Password { get; } =
        configuration["OPCUA_Password"]
        ?? throw new InvalidOperationException("OPCUA_Password configuration is not set.");

    public string SourcePublicId { get; } =
        configuration["IXON_SourcePublicId"]
        ?? throw new InvalidOperationException(
            "IXON_SourcePublicId configuration is not set."
        );

    /// <summary>
    ///     A set of existing variable addresses that have already been created in the IXON platform. This is used to avoid creating duplicate variables when scraping the OPC UA server. The set is populated at the beginning of the execution by fetching the existing variables from the IXON platform for the specified agent.
    /// </summary>
    public HashSet<string> ExistingAddresses { get; private set; } = new();

    /// <summary>
    ///     A set of existing tag public IDs that have already been created in the IXON platform. This is used to avoid creating duplicate tags when scraping the OPC UA server. The set is populated at the beginning of the execution by fetching the existing tags from the IXON platform for the specified agent.
    /// </summary>
    public HashSet<string> ExistingTags { get; private set; } = new();

    /// <summary>
    ///     Executes the scraping process. This method connects to the OPC UA server, browses the full address space, and creates variables and tags in the IXON platform for each relevant node found in the OPC UA server. It checks for existing variables and tags to avoid duplicates and logs the progress of the scraping process.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var client = await clientFactory.Create(Address, Username, Password);
        if (client == null)
        {
            logger.LogError("Failed to create UAClient. Aborting execution.");
            return;
        }

        var sampler = await clientSamplerFactory.CreateAsync();
        var referenceDescriptions = await sampler
            .BrowseFullAddressSpaceAsync(client, Objects.RootFolder, ct: cancellationToken)
            .ConfigureAwait(false);

        ExistingAddresses =
        [
            .. (await apiClient.GetDataVariablesAsync(AgentId)).Data.Select(v => v.Address),
        ];

        ExistingTags =
        [
            .. (await apiClient.GetTagsAsync(AgentId)).Data.Select(t => t.Variable.PublicId),
        ];

        foreach (var referenceDescription in referenceDescriptions)
        {
            if(referenceDescription.NodeId.NamespaceIndex == 0)
            {
                logger.LogDebug(
                    "Skipping node {NodeId} in namespace 0.",
                    referenceDescription.NodeId
                );
                continue;
            }
            var result = await CreateIxonVariableAsync(client,referenceDescription);
            if (result == null)
                continue;

            await CreateTag(result);
        }
    }

    /// <summary>
    ///     Creates a variable in the IXON platform based on the provided OPC UA ReferenceDescription. This method checks if a variable with the same address already exists or if the node is a type definition before attempting to create a new variable. If the variable is successfully created, it returns the created Variable object; otherwise, it returns null.
    /// </summary>
    /// <param name="referenceDescription">
    ///     The ReferenceDescription object representing a node in the OPC UA address space. This object contains information about the node, such as its NodeId, DisplayName, and TypeDefinition, which are used to determine whether to create a new variable and to populate the properties of the variable if it is created.
    /// </param>
    /// <returns>
    ///     A Task that represents the asynchronous operation of creating a variable in the IXON platform. The task result is a Variable object if the variable was successfully created, or null if the variable already exists, is a type definition, or if there was an error during creation.
    /// </returns>
    private async Task<Variable?> CreateIxonVariableAsync(UAClient client,ReferenceDescription referenceDescription)
    {
        if (ExistingAddresses.Contains(referenceDescription.NodeId.ToString()))
        {
            logger.LogInformation(
                "Variable with address {Address} already exists. Skipping creation.",
                referenceDescription.NodeId
            );
            return null;
        }

        if (referenceDescription.NodeId.ToString().Contains("(type)"))
        {
            logger.LogInformation(
                "Node {NodeId} is a type definition. Skipping variable creation.",
                referenceDescription.NodeId
            );
            return null;
        }

        var variable = await MapNodeToVariable(client, referenceDescription);
        if (variable == null)
        {
            return null;
        }

        logger.LogInformation(
            "Creating variable '{VariableName}' with address '{VariableAddress}' and type '{VariableType}'...",
            variable.Name,
            variable.Address,
            variable.Type
        );

        var result = await apiClient.PostVariableAsync(AgentId, variable);
        variable.PublicId = result?.Data.PublicId ?? variable.PublicId;
        return variable;
    }

    /// <summary>
    ///     Creates a tag in the IXON platform for the given variable. This method checks if a tag for the variable already exists before attempting to create a new tag. If the variable does not have a valid PublicId, it returns null. If the tag is successfully created, it returns the created Tag object; otherwise, it returns null.
    /// </summary>
    /// <param name="variable"></param>
    /// <returns></returns>
    private async Task<Tag?> CreateTag(Variable variable)
    {
        if (string.IsNullOrEmpty(variable.PublicId))
        {
            return null;
        }

        var tag = new Tag
        {
            Name = variable.Name,
            Slug = variable.Slug,
            Source = variable.Source,
            Variable = variable,
            RetentionPolicy = "260w",
            LogEvent = "change",
            LoggingInterval = "72s",
        };

        logger.LogInformation(
            "Creating tag for variable '{VariableName}' with PublicId '{VariablePublicId}'...",
            variable.Name,
            variable.PublicId
        );

        var result = await apiClient.PostTagAsync(AgentId, tag);
        return result?.Data;
    }

    /// <summary>
    ///     Maps an OPC UA ReferenceDescription to a Variable object that can be created in the IXON platform. This method determines the built-in type of the node based on its TypeDefinition and maps it to a corresponding type and width for the Variable. If the built-in type cannot be determined, it returns null. The method also generates a slug for the variable based on its display name.
    /// </summary>
    /// <param name="referenceDescription"></param>
    /// <returns></returns>
    private async Task<Variable?> MapNodeToVariable(UAClient client, ReferenceDescription referenceDescription)
    {
        // Only process Variable nodes
        if (referenceDescription.NodeClass != NodeClass.Variable)
        {
            logger.LogDebug(
                "Node {NodeId} is not a Variable (NodeClass: {NodeClass}). Skipping.",
                referenceDescription.NodeId,
                referenceDescription.NodeClass
            );
            return null;
        }
        var dataTypeNodeId = await ReadDataTypeAttributeAsync(client, (NodeId)referenceDescription.NodeId);

        if (dataTypeNodeId == null)
        {
            logger.LogWarning(
                "Could not read DataType attribute for node {NodeId}. Skipping variable creation.",
                referenceDescription.NodeId
            );
            return null;
        }

        var builtInType = GetBuiltInType(dataTypeNodeId);
        if (builtInType == null)
        {
            return null;
        }

        if (builtInType == BuiltInType.String)
        {
            logger.LogDebug(
                "Node {NodeId} is a String type. Skipping as strings are not supported.",
                referenceDescription.NodeId
            );
            return null;
        }

        var type = builtInType switch
        {
            BuiltInType.Boolean => "bool",
            BuiltInType.SByte => "sbyte",
            BuiltInType.Byte => "byte",
            BuiltInType.Int16 => "short",
            BuiltInType.UInt16 => "ushort",
            BuiltInType.Int32 => "int",
            BuiltInType.UInt32 => "uint",
            BuiltInType.Int64 => "long",
            BuiltInType.UInt64 => "ulong",
            BuiltInType.Float => "float",
            BuiltInType.Double => "double",
            BuiltInType.String => "string",
            _ => "Unknown",
        };

        var width = builtInType switch
        {
            BuiltInType.Boolean => null,
            BuiltInType.SByte => "8",
            BuiltInType.Byte => "8",
            BuiltInType.Int16 => "16",
            BuiltInType.UInt16 => "16",
            BuiltInType.Int32 => "32",
            BuiltInType.UInt32 => "32",
            BuiltInType.Int64 => "64",
            BuiltInType.UInt64 => "64",
            BuiltInType.Float => "32",
            BuiltInType.Double => "64",
            BuiltInType.String => "128",
            _ => null,
        };

        if (width == null)
        {
            logger.LogWarning(
                "Warning: The built-in type '{BuiltInType}' does not have a defined width.",
                builtInType
            );
        }

        return new Variable
        {
            Name = referenceDescription.DisplayName.ToString(),
            Address = referenceDescription.NodeId.ToString(),
            Type = type,
            Width = width ,
            Slug = new string([
                .. referenceDescription.DisplayName.ToString().Where(char.IsLetterOrDigit),
            ]).ToLower(),
            Source = new Source { PublicId = SourcePublicId },
            Signed = true,
        };
    }

    /// <summary>
    /// Asynchronously reads the DataType attribute of the specified OPC UA node.
    /// </summary>
    /// <remarks>Returns null if the read operation fails or if the DataType attribute cannot be determined.
    /// The caller should check for a null result before using the returned value.</remarks>
    /// <param name="client">The UAClient instance used to communicate with the OPC UA server. Cannot be null.</param>
    /// <param name="nodeId">The NodeId of the node whose DataType attribute is to be read. Cannot be null.</param>
    /// <returns>A NodeId representing the DataType of the specified node if the read operation succeeds; otherwise, null.</returns>
    private async Task<NodeId> ReadDataTypeAttributeAsync(UAClient client, NodeId nodeId)
    {
        try
        {
            var readValueId = new ReadValueId
            {
                NodeId = nodeId,
                AttributeId = Attributes.DataType,
            };

            var nodesToRead = new ReadValueIdCollection { readValueId };
            var response = await client.Session.ReadAsync(
                null,
                0,
                TimestampsToReturn.Neither,
                nodesToRead,
                CancellationToken.None
            );

            if (response.Results.Count > 0 && response.Results[0].StatusCode == StatusCodes.Good)
            {
                return response.Results[0].Value as NodeId ?? NodeId.Null;
            }
            else
            {
                logger.LogError(
                    "Failed to read DataType attribute for node {NodeId}. StatusCode: {StatusCode}",
                    nodeId,
                    response.Results.Count > 0 ? response.Results[0].StatusCode : StatusCodes.Bad
                );
                return null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading DataType attribute for node {NodeId}", nodeId);
            return null;
        }
    }

    /// <summary>
    ///     Determines the built-in type of an OPC UA node based on its TypeDefinition. This method checks if the TypeDefinition corresponds to a known built-in type and returns the corresponding BuiltInType enum value. If the TypeDefinition does not correspond to a built-in type, it returns null.
    /// </summary>
    /// <param name="dataTypeId"></param>
    /// <returns></returns>
    private BuiltInType? GetBuiltInType(ExpandedNodeId dataTypeId)
    {
        // Check if the NodeId is a numeric identifier
        if (dataTypeId.IdType == IdType.Numeric)
        {
            // The numeric identifier for built-in types starts from 1
            uint numericId = (UInt32)dataTypeId.Identifier;
            if (numericId >= 1 && numericId <= 25) // Assuming there are 25 built-in types
            {
                return (BuiltInType)numericId;
            }
        }

        return null;
    }
}
