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

    public async Task<Response<Variable[]>> GetDataVariablesAsync()
    {
        var uri = new Uri($"/api/agents/{AgentId}/data-variables?fields=address,publicId,name,slug,type,source.publicid&page-size=4000", UriKind.Relative);
        return await GetAsync<Response<Variable[]>>(uri);
    }

    public async Task<Response<Tag[]>> GetTagsAsync()
    {
        var uri = new Uri($"/api/agents/{AgentId}/data-tags?fields=aggregators,edgeAggregator,logEvent,loggingInterval,logTrigger.publicId,name,onChangeExpiry,publicId,retentionPolicy,slug,source.publicId,source.reference.name,tagId,variable.publicId,backendComponent.publicId&page-size=4000", UriKind.Relative);
        return await GetAsync<Response<Tag[]>>(uri);
    }

    public async Task<Response<Tag>?> PostTagAsync(Tag tag)
    {
        var uri = new Uri($"/api/agents/{AgentId}/data-tags", UriKind.Relative);
        return await PostAsync<Response<Tag>>(uri, tag);
    }

    public async Task<Response<Tag[]>?> PostTagsAsync(IEnumerable<Tag> tags)
    {
        var uri = new Uri($"/api/agents/{AgentId}/data-tags", UriKind.Relative);
        return await PostAsync<Response<Tag[]>>(uri, tags);
    }

    public async Task<Response<Tag>?> UpdateTagAsync(string publicId, Tag tag)
    {
        var uri = new Uri($"/api/agents/{AgentId}/data-tags/{publicId}", UriKind.Relative);
        return await PutAsync<Response<Tag>>(uri, tag);
    }

    public async Task<Response<Variable>?> PostVariableAsync(Variable newVariable)
    {
        var uri = new Uri($"/api/agents/{AgentId}/data-variables", UriKind.Relative);
        return await PostAsync<Response<Variable>>(uri, newVariable);
    }

    public async Task<Response<Variable[]>?> PostVariablesAsync(IEnumerable<Variable> variables)
    {
        var uri = new Uri($"/api/agents/{AgentId}/data-variables", UriKind.Relative);
        return await PostAsync<Response<Variable[]>>(uri, variables);
    }

    public async Task<Response<Agent>> GetAgentAsync()
    {
        var uri = new Uri($"/api/agents/{AgentId}?fields=publicId,name,deviceId", UriKind.Relative);
        return await GetAsync<Response<Agent>>(uri);
    }

    public async Task<Response<Device[]>> GetDevicesAsync()
    {
        var uri = new Uri($"/api/agents/{AgentId}/devices?fields=publicId,name,ipAddress,macAddress,customAddress", UriKind.Relative);
        return await GetAsync<Response<Device[]>>(uri);
    }

    public async Task<Response<DataSource[]>> GetDataSourcesAsync()
    {
        var uri = new Uri($"/api/agents/{AgentId}/data-sources?fields=publicId,name,slug,disabled,protocol.publicId,device.publicId&page-size=4000", UriKind.Relative);
        return await GetAsync<Response<DataSource[]>>(uri);
    }

    public async Task<Response<DataSource>?> PostDataSourceAsync(
        DataSource newDataSource
    )
    {
        var uri = new Uri($"/api/agents/{AgentId}/data-sources", UriKind.Relative);
        return await PostAsync<Response<DataSource>>(uri, newDataSource);
    }

    public async Task<Response<Company[]>> GetAssociatedCompanyAsync()
    {
        var uri = new Uri("/api/companies?fields=publicId,name", UriKind.Relative);
        return await GetAsync<Response<Company[]>>(uri);
    }

    public async Task<Response<Agent[]>> GetAgentsAsync()
    {
        var uri = new Uri("/api/agents?fields=publicId,name,deviceId", UriKind.Relative);
        return await GetAsync<Response<Agent[]>>(uri);
    }

    public async Task<Response<string>?> PushConfigurationAsync(string agentId)
    {
        var uri = new Uri("/api/agents/configuration/push", UriKind.Relative);
        return await PostAsync<Response<string>>(uri, new { publicId = agentId });
    }
}
