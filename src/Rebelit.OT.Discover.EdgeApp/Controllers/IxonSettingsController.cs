using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Resolvers;
using Rebelit.OT.Discover.EdgeApp.Services;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IxonSettingsController(
    IAppSettingsManager settingsManager,
    IDataSourceResolver dataSourceResolver,
    IConfiguration configuration
) : ControllerBase
{
    [HttpPost("ixon")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SaveIxonSettings([FromBody] IXONSettings settings)
    {
        settingsManager.Save(new Dictionary<string, string?>
        {
            ["IXON_ApplicationId"] = settings.ApplicationId,
            ["IXON_CompanyId"] = settings.CompanyId,
            ["IXON_AgentId"] = settings.AgentId,
            ["IXON_DataSourceId"] = settings.DataSourceId
        });

        return Ok(new { message = "IXON settings saved successfully." });
    }

    [HttpPost("datasource")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveDataSourceId([FromBody] SaveDataSourceRequest saveDataSourceRequest)
    {
        settingsManager.Save(new Dictionary<string, string?>
        {
            ["IXON_AgentId"] = saveDataSourceRequest.AgentId,
        });

        var settings = settingsManager.Load();
        settings.TryGetValue("IXON_AgentId", out var agentId);

        var result = await dataSourceResolver.ResolveAsync(agentId ?? string.Empty, saveDataSourceRequest.DataSourceName);

        if(string.IsNullOrEmpty(result))
        {
            return BadRequest(new { message = "Failed to resolve data source ID." });
        }

        settingsManager.Save(new Dictionary<string, string?>
        {
            ["IXON_DataSourceId"] = result,
        });

        return Ok(new { message = "Data source ID saved successfully.", dataSourceId = result });
    }

    [HttpGet("ixon")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetIxonSettings() => Ok(new
    {
        applicationId = configuration["IXON_ApplicationId"] ?? string.Empty,
        companyId = configuration["IXON_CompanyId"] ?? string.Empty,
        agentId = configuration["IXON_AgentId"] ?? string.Empty,
        dataSourceId = configuration["IXON_DataSourceId"] ?? string.Empty
    });
}