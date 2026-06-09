using Rebelit.OT.Discover.EdgeApp.API.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

public interface ITagService
{
    /// <summary>
    /// Get all tags that exist on the Ixon fleet manager
    /// </summary>
    Task<IReadOnlyList<Tag>> GetTagsAsync();

    /// <summary>
    /// Returns a list of pre filled tags
    /// Filled with prefilled in values for Ixon Tags
    /// </summary>
    Task<IReadOnlyList<Tag>> GetPrefilledTagsAsync();

    /// <summary>
    /// Create a single tag 
    /// </summary>
    Task<Tag?> CreateTagAsync(Tag request);

    /// <summary>
    /// Create multiple tags at once
    /// </summary>
    Task<IReadOnlyList<Tag>> CreateTagsAsync (IEnumerable<Tag> requests);

    /// <summary>
    /// Update an existing tag on the Ixon fleet manager
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<Tag?> UpdateTagAsync(UpdateTagRequest request);
}
