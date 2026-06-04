using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.API;

public interface IScraper
{
    /// <summary>
    /// Executes the variable scraping operation asynchronously.
    /// This scrapes all the variables from a OPC UA server and creates the corresponding variables in the system.
    /// </summary>
    /// <returns>A task that represents the asynchronous variable scraping operation and returns the created variables.</returns>
    Task<IReadOnlyList<Variable>> ScrapeVariablesAsync(CancellationToken cancellationToken);
}
