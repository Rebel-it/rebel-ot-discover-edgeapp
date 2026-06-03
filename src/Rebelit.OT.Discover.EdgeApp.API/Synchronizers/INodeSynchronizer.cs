using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;

namespace Rebelit.OT.Discover.EdgeApp.API.Synchronizers;

public interface INodeSynchronizer
{
    Task InitializeAsync();

    Task SynchronizeVariablesAsync(string agentId, IEnumerable<Variable> variables);

    /// <summary>
    /// Asynchronously maps an OPC UA reference to a Variable object using the specified client and data source
    /// identifier.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the mapped Variable if successful;
    /// otherwise, null if the mapping could not be performed.</returns>
    Task<Variable?> MapVariableAsync(UAClient client, ReferenceDescription referenceDescription, string dataSourceId);
}