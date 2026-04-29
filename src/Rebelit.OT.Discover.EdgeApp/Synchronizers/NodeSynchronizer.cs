using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;
using Rebelit.OT.Discover.EdgeApp.Mappers;

namespace Rebelit.OT.Discover.EdgeApp.Synchronizers;

public interface INodeSynchronizer
{
    Task InitializeAsync(string agentId);
    Task SynchronizeAsync(UAClient client, ReferenceDescription referenceDescription, string dataSourceId);

    Task SynchronizeVariables(string agentId, IEnumerable<Variable> variables);

    /// <summary>
    /// Asynchronously maps an OPC UA reference to a Variable object using the specified client and data source
    /// identifier.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the mapped Variable if successful;
    /// otherwise, null if the mapping could not be performed.</returns>
    Task<Variable?> MapVariableAsync(UAClient client, ReferenceDescription referenceDescription, string dataSourceId);

    /// <summary>
    /// Creates a new tag associated with the specified variable.
    /// </summary>
    /// <returns>A Tag instance representing the specified variable.</returns>
    Tag CreateTag(Variable variable);
}

internal sealed class NodeSynchronizer(
    IApiClient apiClient,
    IOpcUaVariableMapper variableMapper,
    IConfiguration configuration,
    ILogger<NodeSynchronizer> logger
) : INodeSynchronizer
{
    private readonly string _agentId =
        configuration["IXON_AgentId"]
        ?? throw new InvalidOperationException("IXON_AgentId configuration is not set.");

    private HashSet<string> _existingAddresses = [];
    private HashSet<string> _existingTags = [];

    public async Task InitializeAsync(string agentId)
    {
        _existingAddresses =
        [
            .. (await apiClient.GetDataVariablesAsync(agentId)).Data.Select(v => v.Address),
        ];
        _existingTags =
        [
            .. (await apiClient.GetTagsAsync(agentId)).Data.Select(t => t.Variable.PublicId),
        ];
    }

    public async Task SynchronizeAsync(
        UAClient client,
        ReferenceDescription referenceDescription,
        string dataSourceId
    )
    {
        try
        {
            var variable = await CreateVariableIfNewAsync(client,referenceDescription, dataSourceId);
            if (variable is not null)
                await CreateTagIfNewAsync(variable);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error synchronizing node {NodeId} with address {Address}: {Message}",
                referenceDescription.NodeId,
                referenceDescription.NodeId.ToString(),
                ex.Message
            );
        }
    }

    /// <summary>
    /// Asynchronously reads the DataType attribute of the specified OPC UA node.
    /// </summary>
    /// <returns>A NodeId representing the DataType attribute of the specified node if the read is successful; otherwise, null.</returns>
    private async Task<NodeId?> ReadDataTypeAttributeAsync(
        UAClient client,
        NodeId nodeId
    )
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

            if (response.Results.Count > 0 && response.Results[0].StatusCode == Opc.Ua.StatusCodes.Good)
            {
                return response.Results[0].Value as NodeId ?? NodeId.Null;
            }
            else
            {
                logger.LogError(
                    "Failed to read DataType attribute for node {NodeId}. StatusCode: {StatusCode}",
                    nodeId,
                    response.Results.Count > 0 ? response.Results[0].StatusCode : Opc.Ua.StatusCodes.Bad
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

    private async Task<Variable?> CreateVariableIfNewAsync(
        UAClient client,
        ReferenceDescription referenceDescription,
        string dataSourceId
    )
    {
        var address = referenceDescription.NodeId.ToString();

        if (_existingAddresses.Contains(address))
        {
            logger.LogTrace(
                "Variable with address {Address} already exists. Skipping creation.",
                address
            );
            return null;
        }

        if (address.Contains("(type)"))
        {
            logger.LogTrace(
                "Node {NodeId} is a type definition. Skipping variable creation.",
                address
            );
            return null;
        }

        var dataTypeNodeId = await ReadDataTypeAttributeAsync(client, (NodeId)referenceDescription.NodeId);

        var variable = variableMapper.Map(dataTypeNodeId,referenceDescription, dataSourceId);
        if (variable is null)
        {
            logger.LogWarning(
                "Node '{DisplayName}' ({NodeId}) could not be mapped to an IXON variable and was skipped.",
                referenceDescription.DisplayName,
                referenceDescription.NodeId
            );
            return null;
        }

        logger.LogInformation(
            "Creating variable '{VariableName}' with address '{VariableAddress}' and type '{VariableType}'...",
            variable.Name,
            variable.Address,
            variable.Type
        );

        var result = await apiClient.PostVariableAsync(_agentId, variable);
        variable.PublicId = result?.Data.PublicId ?? variable.PublicId;
        return variable;
    }

    private async Task CreateTagIfNewAsync(Variable variable)
    {
        if (string.IsNullOrEmpty(variable.PublicId))
            return;

        if (_existingTags.Contains(variable.PublicId))
        {
            logger.LogTrace(
                "Tag for variable '{PublicId}' already exists. Skipping creation.",
                variable.PublicId
            );
            return;
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

        await apiClient.PostTagAsync(_agentId, tag);
    }

    public Tag CreateTag(Variable variable)
    {
        if (string.IsNullOrEmpty(variable.Source.PublicId))
            return null;

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

        return tag;
    }

    public async Task<Variable?> MapVariableAsync(UAClient client, ReferenceDescription referenceDescription, string dataSourceId)
    {
        var address = referenceDescription.NodeId.ToString();
        if (_existingAddresses.Contains(address))
        {
            logger.LogTrace(
                "Variable with address {Address} already exists. Skipping creation.",
                address
            );
            return null;
        }

        if (address.Contains("(type)"))
        {
            logger.LogTrace(
                "Node {NodeId} is a type definition. Skipping variable creation.",
                address
            );
            return null;
        }

        var dataTypeNodeId = await ReadDataTypeAttributeAsync(client, (NodeId)referenceDescription.NodeId);
        var variable = variableMapper.Map(dataTypeNodeId, referenceDescription, dataSourceId);

        if (variable is null)
        {
            logger.LogWarning(
                "Node '{DisplayName}' ({NodeId}) could not be mapped to an IXON variable and was skipped.",
                referenceDescription.DisplayName,
                referenceDescription.NodeId
            );
            return null;
        }
        return variable;
    }

    public async Task SynchronizeVariables(string agentId, IEnumerable<Variable> variables)
    {
        var variableList = variables.ToList();
        logger.LogInformation("Posting {Count} variables for agent {AgentId}.", variableList.Count, agentId);
        var result = await apiClient.PostVariablesAsync(agentId, variableList);
        logger.LogInformation(
            "Successfully posted {Count} variables for agent {AgentId}.",
            result?.Data?.Length ?? 0,
            agentId
        );
    }
}
