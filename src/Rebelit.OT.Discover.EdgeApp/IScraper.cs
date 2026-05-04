using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp;

public interface IScraper
{
    Task ExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Executes the variable scraping operation asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous variable scraping operation.</returns>
    Task ExecuteVariableScraperAsync(CancellationToken cancellationToken);
    IReadOnlyList<Variable> CreatedVariables { get; }
    IReadOnlyList<Tag> CreatedTags { get; }
}
