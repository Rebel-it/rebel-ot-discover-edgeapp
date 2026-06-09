using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.API.Mappers;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;

namespace Rebelit.OT.Discover.EdgeApp.API.Synchronizers;

internal sealed class NodeSynchronizer(
    IApiClient apiClient,
    IOpcUaVariableMapper variableMapper,
    ILogger<NodeSynchronizer> logger
) : INodeSynchronizer
{
    private HashSet<string> _existingAddresses = [];

    public async Task InitializeAsync()
    {
        var response = await apiClient.GetDataVariablesAsync();

        _existingAddresses =
        [
            .. (response.Data ?? []).Select(v => v.Address),
        ];
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

            if(client.Session is null)
            {
                if(logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError("OPC UA session is not established. Cannot read DataType attribute for node {NodeId}.", nodeId);
                }
                return null;
            }

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
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(
                    "Failed to read DataType attribute for node {NodeId}. StatusCode: {StatusCode}",
                    nodeId,
                    response.Results.Count > 0 ? response.Results[0].StatusCode : Opc.Ua.StatusCodes.Bad
                );
                }

                return null;
            }
        }
        catch (Exception ex) when (ex is ServiceResultException or TimeoutException or OperationCanceledException)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "Error reading DataType attribute for node {NodeId}", nodeId);
            }
            return null;
        }
    }

    public async Task<Variable?> MapVariableAsync(UAClient client, ReferenceDescription referenceDescription, string dataSourceId)
    {
        var address = referenceDescription.NodeId.ToString();
        if (_existingAddresses.Contains(address))
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace(
                    "Variable with address {Address} already exists. Skipping creation.",
                    address
                );
            }
            return null;
        }

        if (address.Contains("(type)", StringComparison.Ordinal))
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace(
                    "Node {NodeId} is a type definition. Skipping variable creation.",
                    address
                );
            }
            return null;
        }

        var dataTypeNodeId = await ReadDataTypeAttributeAsync(client, (NodeId)referenceDescription.NodeId);

        if (dataTypeNodeId == null)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("ReadDataTypeAttributeAsync failed for node {NodeId}.", referenceDescription.NodeId);
            }
            return null;
        }
        var variable = variableMapper.Map(dataTypeNodeId, referenceDescription, dataSourceId);

        if (variable is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    "Node '{DisplayName}' ({NodeId}) could not be mapped to an IXON variable and was skipped.",
                    referenceDescription.DisplayName,
                    referenceDescription.NodeId
                );
            }
            return null;
        }
        return variable;
    }

    public async Task SynchronizeVariablesAsync(string agentId, IEnumerable<Variable> variables)
    {
        var variableList = variables.ToList();
        if (variableList.Count == 0)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "No variables to post for agent {AgentId}. Skipping synchronization.",
                    agentId
                );
            }
            return;
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Posting {Count} variables for agent {AgentId}.", variableList.Count, agentId);
        }

        var result = await apiClient.PostVariablesAsync(variableList);

        if (result is not null && result.Data is not null)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Successfully posted {Count} variables for agent {AgentId}.",
                    result.Data.Length,
                    agentId
                );
            }
            return;
        }
        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "Posting variables for agent {AgentId} returned an unexpected empty response. Attempted to post {Count} variables.",
                agentId,
                variableList.Count
            );
        }
    }
}
