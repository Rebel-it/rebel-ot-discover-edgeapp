using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Models;

namespace Rebelit.OT.Discover.EdgeApp.Services;

public interface ITagService
{
    Task<IReadOnlyList<Tag>> GetTagsAsync();
    Task<Tag?> UploadTagAsync(Tag tag);
    Task<Tag?> CreateTagAsync(CreateTagRequest request);
}
