using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;

/// <inheritdoc />
internal class ApiClient(
    IOptionsMonitor<Configuration> configuration,
    ILogger<ApiClient> logger,
    TimeProvider timeProvider,
    IIxonAuthenticationContext ixonAuthenticationContext
) : BaseAgent(configuration, logger, timeProvider, ixonAuthenticationContext), IApiClient
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

    public async Task<Response<Variable[]>?> PostVariablesAsync(string agentId, IEnumerable<Variable> variables)
    {
        var uri = $"/api/agents/{agentId}/data-variables";
        return await Post<Response<Variable[]>>(uri, variables);
    }

    public async Task<Response<Agent>> GetAgentAsync(string agentId)
    {
        var uri = $"/api/agents/{agentId}?fields=publicId,name,deviceId";
        return await Get<Response<Agent>>(uri);
    }

    public async Task<Response<Device[]>> GetDevicesAsync(string agentId)
    {
        var uri =
            $"/api/agents/{agentId}/devices?fields=publicId,name,ipAddress,macAddress,customAddress";
        return await Get<Response<Device[]>>(uri);
    }

    public async Task<Response<DataSource[]>> GetDataSourcesAsync(string agentId)
    {
        var uri =
            $"/api/agents/{agentId}/data-sources?fields=publicId,name,slug,disabled,protocol.publicId,device.publicId&page-size=4000";
        return await Get<Response<DataSource[]>>(uri);
    }

    public async Task<Response<DataSource>?> PostDataSourceAsync(
        string agentId,
        DataSource newDataSource
    )
    {
        var uri = $"/api/agents/{agentId}/data-sources";
        return await Post<Response<DataSource>>(uri, newDataSource);
    }

    public async Task<Response<Company[]>> GetAssociatedCompanyAsync()
    {
        const string uri = "/api/companies?fields=publicId,name";
        return await Get<Response<Company[]>>(uri);
    }

    public async Task<Response<Agent[]>> GetAgentsAsync()
    {
        const string uri = "/api/agents?fields=publicId,name,deviceId";
        return await Get<Response<Agent[]>>(uri);
    }
}
