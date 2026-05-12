using Rebelit.OT.Discover.EdgeApp.API.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

public interface ITagService
{
    Task<IReadOnlyList<Tag>> GetTagsAsync();
    Task<Tag?> UploadTagAsync(Tag tag);
    Task<Tag?> CreateTagAsync(CreateTagRequest request);
}
