using Rebelit.OT.Discover.EdgeApp.API.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

internal sealed class TagService(
    IApiClient apiClient,
    IConfiguration configuration,
    ILogger<TagService> logger) : ITagService
{
    private readonly string _agentId =
        configuration["IXON_AgentId"]
        ?? throw new InvalidOperationException("IXON_AgentId configuration is not set.");

    public async Task<IReadOnlyList<Tag>> GetTagsAsync()
    {
        var response = await apiClient.GetTagsAsync(_agentId);
        var tags = response.Data ?? [];

        logger.LogInformation("Retrieved {Count} tags for agent {AgentId}.", tags.Length, _agentId);
        return tags;
    }

    public async Task<Tag?> UploadTagAsync(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        var response = await apiClient.PostTagAsync(_agentId, tag);
        var createdTag = response?.Data;

        logger.LogInformation(
            "Created tag {TagName} for agent {AgentId}.",
            createdTag?.Name ?? tag.Name,
            _agentId);

        return createdTag;
    }

    public async Task<Tag?> CreateTagAsync(CreateTagRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tag = MapToTag(request);
        return await UploadTagAsync(tag);
    }

    private static Tag MapToTag(CreateTagRequest request) => new()
    {
        Name = request.Name,
        Slug = request.Slug,
        LogEvent = request.LogEvent,
        LoggingInterval = request.LoggingInterval,
        OnChangeExpiry = request.OnChangeExpiry,
        RetentionPolicy = request.RetentionPolicy,
        EdgeAggregator = request.EdgeAggregator,
        Variable = new TagVariable { PublicId = request.Variable },
    };
}
