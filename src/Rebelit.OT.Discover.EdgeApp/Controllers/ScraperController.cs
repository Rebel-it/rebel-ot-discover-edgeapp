using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Exporters;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

public class ScraperController(IScraper scraper, ICsvExporters csvExporters) : BaseController
{
    [HttpPost("variables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunVariableScrapeAsync(CancellationToken cancellationToken)
    {
        await scraper.ExecuteVariableScraperAsync(cancellationToken);
        return Ok(new
        {
            count = scraper.CreatedVariables.Count,
        });
    }
}
