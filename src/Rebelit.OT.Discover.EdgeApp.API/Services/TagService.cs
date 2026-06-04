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
    private const string DefaultLogEvent = "interval";
    private const string DefaultLoggingInterval = "500ms";
    private const string DefaultRetentionPolicy = "260w";
    private const string DefaultEdgeAggregator = "last";

    public async Task<IReadOnlyList<Tag>> GetTagsAsync()
    {
        var response = await apiClient.GetTagsAsync();
        var tags = response.Data ?? [];

        if(logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Retrieved {Count} tags for agent {AgentId}.", tags.Length, ixonAuthenticationContext.IxonHeaders.AgentId);
        }

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
                LogEvent = DefaultLogEvent,
                LoggingInterval = DefaultLoggingInterval,
                RetentionPolicy = DefaultRetentionPolicy,
                EdgeAggregator = DefaultEdgeAggregator,
                Variable = new TagVariable { PublicId = variable.PublicId },
            };
            if(existingTags.Any(t => t.Variable.PublicId == variable.PublicId || !variable.Address.Contains("ns=", StringComparison.Ordinal)))
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

    public async Task CreateTagsAsync(IEnumerable<Tag> requests)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Posting {Count} tags for agent {AgentId}.", requests.Count(), ixonAuthenticationContext.IxonHeaders.AgentId);
        }
        var result = await apiClient.PostTagsAsync(requests);
        
        if (result is not null && result.Data is not null)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Successfully posted {Count} tags for agent {AgentId}.",
                    result.Data.Length,
                    ixonAuthenticationContext.IxonHeaders.AgentId
                );
            }

            return;
        }

        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "Posting tags for agent {AgentId} returned an unexpected empty response. Attempted to post {Count} tags.",
                ixonAuthenticationContext.IxonHeaders.AgentId,
                requests.Count()
            );
        }
    }

    public async Task<Tag?> CreateTagAsync(Tag request)
    {
        var response = await apiClient.PostTagAsync(request);
        var createdTag = response?.Data;
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Created tag {TagName} for agent {AgentId}.",
                createdTag?.Name ?? request.Name,
                ixonAuthenticationContext.IxonHeaders.AgentId);
        }
        return createdTag;
    }

    public async Task<Tag?> UpdateTagAsync(UpdateTagRequest request)
    {
        var tag = new Tag
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
        var response = await apiClient.UpdateTagAsync(request.PublicId, tag);
        var updatedTag = response?.Data;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                 "Updated tag {TagName} for agent {AgentId}.",
                 updatedTag?.Name ?? tag.Name,
                 ixonAuthenticationContext.IxonHeaders.AgentId);
        }

        return updatedTag;
    }

}
