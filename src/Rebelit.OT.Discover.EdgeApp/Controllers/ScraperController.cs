using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Exporters;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScraperController(IScraper scraper, ICsvExporters csvExporters) : ControllerBase
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
