using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Services;

public interface ITagService
{
    Task<IReadOnlyList<Tag>> GetTagsAsync(CancellationToken cancellationToken = default);
    Task<Tag?> CreateTagAsync(Tag tag, CancellationToken cancellationToken = default);
}
