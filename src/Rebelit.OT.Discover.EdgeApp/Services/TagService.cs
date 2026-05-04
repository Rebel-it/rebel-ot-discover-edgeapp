using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Services;

internal sealed class TagService(
    IApiClient apiClient,
    IConfiguration configuration,
    ILogger<TagService> logger) : ITagService
{
    private readonly string _agentId =
        configuration["IXON_AgentId"]
        ?? throw new InvalidOperationException("IXON_AgentId configuration is not set.");

    public async Task<IReadOnlyList<Tag>> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        var response = await apiClient.GetTagsAsync(_agentId);
        var tags = response.Data ?? [];

        logger.LogInformation("Retrieved {Count} tags for agent {AgentId}.", tags.Length, _agentId);
        return tags;
    }

    public async Task<Tag?> CreateTagAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (string.IsNullOrWhiteSpace(tag.Name))
            throw new ArgumentException("Tag name is required.", nameof(tag));

        if (tag.Variable is null || string.IsNullOrWhiteSpace(tag.Variable.PublicId))
            throw new ArgumentException("Tag variable public id is required.", nameof(tag));

        if (tag.Source is null || string.IsNullOrWhiteSpace(tag.Source.PublicId))
            throw new ArgumentException("Tag source public id is required.", nameof(tag));

        var response = await apiClient.PostTagAsync(_agentId, tag);
        var createdTag = response?.Data;

        logger.LogInformation(
            "Created tag {TagName} for agent {AgentId}.",
            createdTag?.Name ?? tag.Name,
            _agentId);

        return createdTag;
    }
}
