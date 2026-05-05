using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[ApiController]
public class PlcController(
    IUAClientFactory uaClientFactory
) : BaseController
{
    [HttpPost("connect")]
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

        // settingsManager.Save(new Dictionary<string, string?>
        // {
        //     ["OPCUA_ServerAddress"] = settings.OpcUaServerAddress,
        //     ["OPCUA_Username"] = settings.OpcUaUsername,
        //     ["OPCUA_Password"] = settings.OpcUaPassword
        // });
        return Ok(new { message = "OPC UA settings saved successfully." });
    }
}