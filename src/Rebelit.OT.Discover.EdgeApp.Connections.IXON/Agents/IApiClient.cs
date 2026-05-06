using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;

/// <summary>
///     Interface for IXON API client, defining methods for interacting with IXON agents, variables, and tags.
/// </summary>
public interface IApiClient
{
    /// <summary>
    ///     Set a Datasource Variable.
    /// </summary>
    /// <param name="newVariable"></param>
    /// <returns></returns>
    Task<Response<Variable>?> PostVariableAsync(string agentId, Variable newVariable);

    /// <summary>
    /// Posts a collection of variables to the specified agent asynchronously.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent to which the variables will be posted. Cannot be null or empty.</param>
    /// <param name="variables">An IEnumerable of Variable objects to be posted.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a response with an array of
    /// variables that were posted. If the operation fails, an exception is thrown.</returns>
    Task<Response<Variable[]>?> PostVariablesAsync(string agentId, IEnumerable<Variable> variables);

    /// <summary>
    ///     Get all Variables for a given Agent.
    /// </summary>
    /// <param name="agentId"></param>
    /// <returns></returns>
    Task<Response<Variable[]>> GetDataVariablesAsync(string agentId);

    /// <summary>
    ///     Get all Tags for a given Agent.
    /// </summary>
    /// <param name="agentId"></param>
    /// <returns></returns>
    Task<Response<Tag[]>> GetTagsAsync(string agentId);

    /// <summary>
    ///     Set a Tag.
    /// </summary>
    /// <param name="agentId"></param>
    /// <param name="newTag"></param>
    /// <returns></returns>
    Task<Response<Tag>?> PostTagAsync(string agentId, Tag newTag);

    /// <summary>
    ///     Gets all data sources for the specified agent.
    /// </summary>
    /// <param name="agentId">The public ID of the agent.</param>
    Task<Response<DataSource[]>> GetDataSourcesAsync(string agentId);

    /// <summary>
    ///     Gets the agent details for the specified agent.
    /// </summary>
    /// <param name="agentId">The public ID of the agent.</param>
    Task<Response<Agent>> GetAgentAsync(string agentId);

    /// <summary>
    ///     Gets the devices associated with the specified agent.
    /// </summary>
    /// <param name="agentId">The public ID of the agent.</param>
    Task<Response<Device[]>> GetDevicesAsync(string agentId);

    /// <summary>
    ///     Creates a new data source for the specified agent.
    /// </summary>
    /// <param name="agentId">The public ID of the agent.</param>
    /// <param name="newDataSource">The data source to create.</param>
    Task<Response<DataSource>?> PostDataSourceAsync(string agentId, DataSource newDataSource);
    
    /// <summary>
    ///     Get company associated with service account.
    ///     A service account can be associated with only one company.
    /// </summary>
    /// <returns></returns>
    Task<Response<Company?>?> GetAssociatedCompanyAsync();
    
    /// <summary>
    ///     Gets a list of agents associated with the companyId.
    /// </summary>
    /// <returns></returns>
    Task<Response<Agent[]>> GetAgentsAsync();
}
