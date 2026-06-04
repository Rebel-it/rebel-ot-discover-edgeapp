using Microsoft.AspNetCore.Mvc;

namespace Rebelit.OT.Discover.EdgeApp.API.Controllers;

public class ScraperController(IScraper scraper) : BaseController
{
    [HttpPost("variables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunVariableScrapeAsync(CancellationToken cancellationToken)
    {
        var createdVariables = await scraper.ScrapeVariablesAsync(cancellationToken);
        return Ok(new
        {
            count = createdVariables.Count,
        });
    }
}
