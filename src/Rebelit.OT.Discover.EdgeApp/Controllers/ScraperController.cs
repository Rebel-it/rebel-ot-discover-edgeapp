using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Exporters;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScraperController(IScraper scraper, ICsvExporters csvExporters) : ControllerBase
{
    [HttpPost("run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunAsync(CancellationToken cancellationToken)
    {
        await scraper.ExecuteAsync(cancellationToken);
        return Ok(new
        {
            variables = scraper.CreatedVariables.Count,
            tags = scraper.CreatedTags.Count
        });
    }

    [HttpGet("variables/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadVariablesCsvAsync(CancellationToken cancellationToken)
    {
        await scraper.ExecuteAsync(cancellationToken);
        var csv = csvExporters.CreateVariableCsv(scraper.CreatedVariables.ToList());
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"variables_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet("tags/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadTagsCsvAsync(CancellationToken cancellationToken)
    {
        await scraper.ExecuteAsync(cancellationToken);
        var csv = csvExporters.CreateTagCsv(scraper.CreatedTags.ToList());
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"tags_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

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
