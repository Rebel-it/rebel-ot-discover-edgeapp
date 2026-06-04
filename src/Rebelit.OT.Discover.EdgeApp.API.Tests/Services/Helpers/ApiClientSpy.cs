using System.Text.Json;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Services.Helpers;

public sealed class ApiClientSpy : IApiClient
{
    public Variable[]? DataVariables { get; set; }
    public Tag[]? Tags { get; set; }
    public Tag[]? PostedTagsResponse { get; set; }
    public Tag? PostedTagResponse { get; set; }
    public Tag? UpdatedTagResponse { get; set; }
    public bool ReturnNullFromPostTags { get; set; }
    public bool ReturnNullFromPostTag { get; set; }
    public bool ReturnNullFromUpdateTag { get; set; }
    public IReadOnlyList<Tag>? PostedTags { get; private set; }
    public Tag? PostedTag { get; private set; }
    public Tag? UpdatedTagRequest { get; private set; }
    public string? UpdatedTagPublicId { get; private set; }

    public int GetDataVariablesCallCount { get; private set; }
    public int GetTagsCallCount { get; private set; }
    public int PostTagsCallCount { get; private set; }
    public int PostTagCallCount { get; private set; }
    public int UpdateTagCallCount { get; private set; }

    public Task<Response<Variable[]>> GetDataVariablesAsync()
    {
        GetDataVariablesCallCount++;
        return Task.FromResult(new Response<Variable[]>
        {
            RawData = JsonSerializer.SerializeToElement(DataVariables),
        });
    }

    public Task<Response<Tag[]>> GetTagsAsync()
    {
        GetTagsCallCount++;
        return Task.FromResult(new Response<Tag[]>
        {
            RawData = JsonSerializer.SerializeToElement(Tags),
        });
    }

    public Task<Response<Variable>?> PostVariableAsync(Variable newVariable) =>
        throw new NotSupportedException();

    public Task<Response<Variable[]>?> PostVariablesAsync(IEnumerable<Variable> variables) =>
        throw new NotSupportedException();

    public Task<Response<Tag>?> PostTagAsync(Tag tag)
    {
        PostTagCallCount++;
        PostedTag = tag;

        if (ReturnNullFromPostTag)
        {
            return Task.FromResult<Response<Tag>?>(null);
        }

        return Task.FromResult<Response<Tag>?>(new Response<Tag>
        {
            RawData = JsonSerializer.SerializeToElement(PostedTagResponse),
        });
    }

    public Task<Response<Tag[]>?> PostTagsAsync(IEnumerable<Tag> tags)
    {
        PostTagsCallCount++;
        PostedTags = tags.ToArray();

        if (ReturnNullFromPostTags)
        {
            return Task.FromResult<Response<Tag[]>?>(null);
        }

        return Task.FromResult<Response<Tag[]>?>(new Response<Tag[]>
        {
            RawData = JsonSerializer.SerializeToElement(PostedTagsResponse),
        });
    }

    public Task<Response<Tag>?> UpdateTagAsync(string publicId, Tag tag)
    {
        UpdateTagCallCount++;
        UpdatedTagPublicId = publicId;
        UpdatedTagRequest = tag;

        if (ReturnNullFromUpdateTag)
        {
            return Task.FromResult<Response<Tag>?>(null);
        }

        return Task.FromResult<Response<Tag>?>(new Response<Tag>
        {
            RawData = JsonSerializer.SerializeToElement(UpdatedTagResponse),
        });
    }

    public Task<Response<DataSource[]>> GetDataSourcesAsync() =>
        throw new NotSupportedException();

    public Task<Response<Agent>> GetAgentAsync() =>
        throw new NotSupportedException();

    public Task<Response<Device[]>> GetDevicesAsync() =>
        throw new NotSupportedException();

    public Task<Response<DataSource>?> PostDataSourceAsync(DataSource newDataSource) =>
        throw new NotSupportedException();

    public Task<Response<Company[]>> GetAssociatedCompanyAsync() =>
        throw new NotSupportedException();

    public Task<Response<Agent[]>> GetAgentsAsync() =>
        throw new NotSupportedException();

    public Task<Response<string>?> PushConfigurationAsync(string agentId) =>
        throw new NotSupportedException();
}
