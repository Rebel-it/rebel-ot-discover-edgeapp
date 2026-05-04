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
        var result = await scraper.ExecuteAsync(cancellationToken);
        return Ok(new
        {
            variables = result.Variables.Count,
            tags = result.Tags.Count
        });
    }

    [HttpGet("variables/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadVariablesCsvAsync(CancellationToken cancellationToken)
    {
        var result = await scraper.ExecuteAsync(cancellationToken);
        var csv = csvExporters.CreateVariableCsv(result.Variables.ToList());
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"variables_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet("tags/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadTagsCsvAsync(CancellationToken cancellationToken)
    {
        var result = await scraper.ExecuteAsync(cancellationToken);
        var csv = csvExporters.CreateTagCsv(result.Tags.ToList());
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"tags_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    [HttpPost("variables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunVariableScrapeAsync(CancellationToken cancellationToken)
    {
        var variables = await scraper.ExecuteVariableScraperAsync(cancellationToken);
        return Ok(new { count = variables.Count });
    }
}
