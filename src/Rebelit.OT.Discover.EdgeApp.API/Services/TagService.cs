using Rebelit.OT.Discover.EdgeApp.API.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

internal sealed class TagService(
    IApiClient apiClient,
    IIxonAuthenticationContext ixonAuthenticationContext,
     IConfiguration configuration,
     IVariableService variableService,
    ILogger<TagService> logger) : ITagService
{
    public async Task<IReadOnlyList<Tag>> GetTagsAsync()
    {
        var response = await apiClient.GetTagsAsync();
        var tags = response.Data ?? [];

        logger.LogInformation("Retrieved {Count} tags for agent {AgentId}.", tags.Length, ixonAuthenticationContext.IxonHeaders.AgentId);
        return tags;
    }
    public async Task<IReadOnlyList<Tag>> GetPrefilledTagsAsync()
    {
        var variables = await variableService.GetVariablesAsync();
        List<Tag> tags = new List<Tag>();

        foreach (var variable in variables)
        {
            var tag = new Tag
            {
                Name = variable.Name,
                Slug = variable.Name.ToLower().Replace(" ", "-"),
                LogEvent = "interval",
                LoggingInterval = "500ms",
                RetentionPolicy = "260w",
                EdgeAggregator = "last",
                Variable = new TagVariable { PublicId = variable.PublicId },
            };
            tags.Add(tag);
        }

        return tags;
    }

    public async Task<Tag?> UploadTagAsync(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        var response = await apiClient.PostTagAsync(tag);
        var createdTag = response?.Data;

        logger.LogInformation(
            "Created tag {TagName} for agent {AgentId}.",
            createdTag?.Name ?? tag.Name,
            ixonAuthenticationContext.IxonHeaders.AgentId);

        return createdTag;
    }

    public async Task<Tag?> UpdateTagAsync(Tag tag, string publicId)
    {
        ArgumentNullException.ThrowIfNull(tag);
        var response = await apiClient.UpdateTagAsync(publicId, tag);
        var updatedTag = response?.Data;

        logger.LogInformation(
             "Updated tag {TagName} for agent {AgentId}.",
             updatedTag?.Name ?? tag.Name,
             ixonAuthenticationContext.IxonHeaders.AgentId);

        return updatedTag;
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

    public async Task<Tag?> UpdateTagAsync(UpdateTagRequest request)
    {
        var tag = MapToTag(request);
        ArgumentNullException.ThrowIfNull(tag);
        var response = await apiClient.UpdateTagAsync(request.PublicId, tag);
        var updatedTag = response?.Data;

        logger.LogInformation(
             "Updated tag {TagName} for agent {AgentId}.",
             updatedTag?.Name ?? tag.Name,
             ixonAuthenticationContext.IxonHeaders.AgentId);

        return updatedTag;
    }
}
