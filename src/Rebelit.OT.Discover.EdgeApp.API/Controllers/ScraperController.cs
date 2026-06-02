using Microsoft.AspNetCore.Mvc;

namespace Rebelit.OT.Discover.EdgeApp.API.Controllers;

public class ScraperController(IScraper scraper) : BaseController
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
