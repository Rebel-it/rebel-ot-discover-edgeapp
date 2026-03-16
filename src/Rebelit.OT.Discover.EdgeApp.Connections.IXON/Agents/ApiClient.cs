using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;

/// <inheritdoc />
internal class ApiClient(IOptions<Configuration> configuration, ILogger<ApiClient> logger)
    : BaseAgent(configuration, logger),
        IApiClient
{
    public async Task<Response<Variable[]>> GetDataVariablesAsync(string agentId)
    {
        var uri = $"/api/agents/{agentId}/data-variables?fields=address,publicId&page-size=4000";
        return await Get<Response<Variable[]>>(uri);
    }

    public async Task<Response<Tag[]>> GetTagsAsync(string agentId)
    {
        var uri =
            $"/api/agents/{agentId}/data-tags?fields=aggregators,edgeAggregator,logEvent,loggingInterval,logTrigger.publicId,name,onChangeExpiry,publicId,retentionPolicy,slug,source.publicId,source.reference.name,tagId,variable.publicId,backendComponent.publicId&page-size=4000";
        return await Get<Response<Tag[]>>(uri);
    }

    public async Task<Response<Tag>?> PostTagAsync(string agentId, Tag newTag)
    {
        var uri = $"/api/agents/{agentId}/data-tags";
        return await Post<Response<Tag>>(uri, newTag);
    }

    public async Task<Response<Variable>?> PostVariableAsync(string agentId, Variable newVariable)
    {
        var uri = $"/api/agents/{agentId}/data-variables";
        return await Post<Response<Variable>>(uri, newVariable);
    }
}
