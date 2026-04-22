using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Services;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IxonSettingsController(
    ISettingsManager settingsManager,
    IConfiguration configuration
) : ControllerBase
{
    [HttpPost("ixon")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SaveIxonSettings([FromBody] IXONSettings settings)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(settings.ApplicationId))
            errors.Add("ApplicationId is required.");
        if (string.IsNullOrWhiteSpace(settings.ComnpanyId))
            errors.Add("CompanyId is required.");
        if (string.IsNullOrWhiteSpace(settings.AgentId))
            errors.Add("AgentId is required.");
        if (string.IsNullOrWhiteSpace(settings.OpcUaServerAddress))
            errors.Add("OpcUaServerAddress is required.");
        if (string.IsNullOrWhiteSpace(settings.OpcUaUsername))
            errors.Add("OpcUaUsername is required.");
        if (string.IsNullOrWhiteSpace(settings.OpcUaPassword))
            errors.Add("OpcUaPassword is required.");

        if (errors.Count > 0)
            return BadRequest(new { errors });

        settingsManager.Save(new Dictionary<string, string?>
        {
            ["IXON_ApplicationId"] = settings.ApplicationId,
            ["IXON_CompanyId"] = settings.ComnpanyId,
            ["IXON_AgentId"] = settings.AgentId,
            ["IXON_DataSourceId"] = settings.DataSourceID,
            ["OPCUA_ServerAddress"] = settings.OpcUaServerAddress,
            ["OPCUA_Username"] = settings.OpcUaUsername,
            ["OPCUA_Password"] = settings.OpcUaPassword,
        });

        return Ok(new { message = "IXON settings saved successfully." });
    }

    [HttpGet("ixon")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetIxonSettings() => Ok(new
    {
        applicationId = configuration["IXON_ApplicationId"] ?? string.Empty,
        companyId = configuration["IXON_CompanyId"] ?? string.Empty,
        agentId = configuration["IXON_AgentId"] ?? string.Empty,
        dataSourceId = configuration["IXON_DataSourceId"] ?? string.Empty,
        opcUaServerAddress = configuration["OPCUA_ServerAddress"] ?? string.Empty,
        opcUaUsername = configuration["OPCUA_Username"] ?? string.Empty,
    });
}