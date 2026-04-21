using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp;

public interface IScraper
{
    Task ExecuteAsync(CancellationToken cancellationToken);
    IReadOnlyList<Variable> CreatedVariables { get; }
    IReadOnlyList<Tag> CreatedTags { get; }
}
