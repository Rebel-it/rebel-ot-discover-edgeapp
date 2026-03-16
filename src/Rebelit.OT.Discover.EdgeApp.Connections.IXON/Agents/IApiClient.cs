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
}
