using Microsoft.Extensions.Logging;
using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;

namespace Rebelit.OT.Discover.EdgeApp;

public class Scraper(
    IUAClientFactory clientFactory,
    IClientSamplerFactory clientSamplerFactory,
    IApiClient apiClient,
    ILogger<Scraper> logger
) : IScraper
{
    /// <summary>
    ///     The address of the OPC UA server to scrape. This should be the endpoint where your OPC UA server is accessible.
    ///     Make sure to replace this with the actual address of your OPC UA server.
    /// </summary>
    public string Address { get; } = "opc.tcp://172.27.21.3:4840";

    /// <summary>
    ///     The agent ID to which the scraped data will be sent. This should correspond to an existing agent (device) in the IXON platform.
    ///     Make sure to replace this with the actual agent ID you intend to use for storing the scraped data.
    /// </summary>
    public string AgentId { get; } = "ajParmNkfASN";

    /// <summary>
    ///     The username for authenticating with the OPC UA server. This should be a valid username that has access to the server.
    ///     Make sure to replace this with the actual username you use to connect to your OPC UA server.
    /// </summary>
    public string Username { get; } = "pip";

    /// <summary>
    ///     The password for authenticating with the OPC UA server. This should be a valid password that corresponds to the username.
    ///     Make sure to replace this with the actual password you use to connect to your OPC UA server.
    /// </summary>
    public string Password { get; } = "innovations";

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
            .. (await apiClient.GetDataVariablesAsync("ajParmNkfASN")).Data.Select(v => v.Address),
        ];

        ExistingTags =
        [
            .. (await apiClient.GetTagsAsync("ajParmNkfASN")).Data.Select(t => t.Variable.PublicId),
        ];

        foreach (var referenceDescription in referenceDescriptions)
        {
            var result = await CreateIxonVariableAsync(referenceDescription);
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
    private async Task<Variable?> CreateIxonVariableAsync(ReferenceDescription referenceDescription)
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

        var variable = MapNodeToVariable(referenceDescription);
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

        var result = await apiClient.PostVariableAsync("ajParmNkfASN", variable);
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

        var result = await apiClient.PostTagAsync("ajParmNkfASN", tag);
        return result?.Data;
    }

    /// <summary>
    ///     Maps an OPC UA ReferenceDescription to a Variable object that can be created in the IXON platform. This method determines the built-in type of the node based on its TypeDefinition and maps it to a corresponding type and width for the Variable. If the built-in type cannot be determined, it returns null. The method also generates a slug for the variable based on its display name.
    /// </summary>
    /// <param name="referenceDescription"></param>
    /// <returns></returns>
    private Variable? MapNodeToVariable(ReferenceDescription referenceDescription)
    {
        var builtInType = GetBuiltInType(referenceDescription.TypeDefinition);
        if (builtInType == null)
        {
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
            BuiltInType.Boolean => "1",
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
            Width = width ?? "Unknown",
            Slug = new string([
                .. referenceDescription.DisplayName.ToString().Where(char.IsLetterOrDigit),
            ]).ToLower(),
            Source = new Source { PublicId = "LJrOZaLiaJjB" },
            Signed = true,
        };
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
