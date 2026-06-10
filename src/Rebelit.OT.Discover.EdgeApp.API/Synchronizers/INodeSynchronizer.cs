using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;

namespace Rebelit.OT.Discover.EdgeApp.API.Synchronizers;

public interface INodeSynchronizer
{
    /// <summary>
    /// Get the current existing variables for the given data source in the system and store them in memory for later use during the synchronization process.
    /// </summary>
    /// <param name="dataSourceId">The identifier of the data source to retrieve variables for.</param>
    /// <returns></returns>
    Task InitializeAsync(string dataSourceId);

    /// <summary>
    ///  Synchronizes the provided variables with the existing variables in the system for the specified agent. This method compares the provided variables with the existing ones and performs necessary create, update, or delete operations to ensure that the system's state reflects the provided variables accurately.
    /// </summary>
    /// <param name="agentId">The identifier of the agent for which the variables are being synchronized.</param>
    /// <param name="variables">The collection of variables to synchronize.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SynchronizeVariablesAsync(string agentId, IEnumerable<Variable> variables);

    /// <summary>
    /// Asynchronously maps an OPC UA reference to a Variable object using the specified client and data source
    /// identifier.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the mapped Variable if successful;
    /// otherwise, null if the mapping could not be performed.</returns>
    Task<Variable?> MapVariableAsync(UAClient client, ReferenceDescription referenceDescription, string dataSourceId);
}