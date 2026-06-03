using Rebelit.OT.Discover.EdgeApp.API.Models;
using Rebelit.OT.Discover.EdgeApp.API.Utilities;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

public sealed class TagService(
    IApiClient apiClient,
    IIxonAuthenticationContext ixonAuthenticationContext,
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
        var existingTags = await GetExistingTagsAsync();
        List<Tag> tags = [];

        foreach (var variable in variables)
        {
            if(string.IsNullOrWhiteSpace(variable.PublicId))
            {
                continue;
            }
            var tag = new Tag
            {
                Name = variable.Name,
                Slug = SlugGenerator.CreateFromNameAndAddress(variable.Name, variable.Address),
                LogEvent = "interval",
                LoggingInterval = "500ms",
                RetentionPolicy = "260w",
                EdgeAggregator = "last",
                Variable = new TagVariable { PublicId = variable.PublicId },
            };
            if(existingTags.Any(t => t.Variable.PublicId == variable.PublicId || !variable.Address.Contains("ns=")))
            {
                continue; 
            }
            tags.Add(tag);
        }

        return tags;
    }
    public async Task<IReadOnlyList<Tag>> GetExistingTagsAsync()
    {
        var response = await apiClient.GetTagsAsync();
        var tags = response.Data ?? [];

        return tags;
    }

    public async Task<Tag?> UploadTagAsync(Tag tag)
    {
        var response = await apiClient.PostTagAsync(tag);
        var createdTag = response?.Data;

        logger.LogInformation(
            "Created tag {TagName} for agent {AgentId}.",
            createdTag?.Name ?? tag.Name,
            ixonAuthenticationContext.IxonHeaders.AgentId);

        return createdTag;
    }
    public async Task CreateTagsAsync(IEnumerable<Tag> requests)
    {
        logger.LogInformation("Posting {Count} tags for agent {AgentId}.", requests.Count(), ixonAuthenticationContext.IxonHeaders.AgentId);
        var result = await apiClient.PostTagsAsync(requests);

        if (result is not null && result.Data is not null)
        {
            logger.LogInformation(
                "Successfully posted {Count} tags for agent {AgentId}.",
                result.Data.Length,
                ixonAuthenticationContext.IxonHeaders.AgentId
            );
            return;
        }

        logger.LogWarning(
            "Posting tags for agent {AgentId} returned an unexpected empty response. Attempted to post {Count} tags.",
            ixonAuthenticationContext.IxonHeaders.AgentId,
            requests.Count()
            );   
    }

    public async Task<Tag?> CreateTagAsync(Tag request)
    {
        return await UploadTagAsync(request);
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
        var response = await apiClient.UpdateTagAsync(request.PublicId, tag);
        var updatedTag = response?.Data;

        logger.LogInformation(
             "Updated tag {TagName} for agent {AgentId}.",
             updatedTag?.Name ?? tag.Name,
             ixonAuthenticationContext.IxonHeaders.AgentId);

        return updatedTag;
    }

}
