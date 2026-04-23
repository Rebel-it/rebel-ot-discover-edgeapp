using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;
using Rebelit.OT.Discover.EdgeApp.Services;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[ApiController]
[Route("api/[controller]")]

public class OpcController(
    IAppSettingsManager settingsManager,
    IConfiguration configuration,
    IUAClientFactory uaClientFactory
) : ControllerBase
{
    [HttpPost("opc")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveOpcSettings([FromBody] OpcCredentials settings)
    {
        using var client = await uaClientFactory.Create(
            settings.OpcUaServerAddress,
            settings.OpcUaUsername ?? string.Empty,
            settings.OpcUaPassword ?? string.Empty
        );

        if (client is null)
        {
            return BadRequest(new { message = "Failed to connect to the OPC UA server with the provided settings." });
        }

        settingsManager.Save(new Dictionary<string, string?>
        {
            ["OPCUA_ServerAddress"] = settings.OpcUaServerAddress,
            ["OPCUA_Username"] = settings.OpcUaUsername,
            ["OPCUA_Password"] = settings.OpcUaPassword
        });
        return Ok(new { message = "OPC UA settings saved successfully." });
    }
    [HttpGet("opc")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetOpcSettings() => Ok(new
    {
        serverAddress = configuration["OPCUA_ServerAddress"] ?? string.Empty,
        username = configuration["OPCUA_Username"] ?? string.Empty,
        password = configuration["OPCUA_Password"] ?? string.Empty
    });
}