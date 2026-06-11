using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;

namespace Rebelit.OT.Discover.EdgeApp.API.Controllers;

public class PlcController(
    IUAClientFactory uaClientFactory
) : BaseController
{
    [HttpPost("Connect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestOpcConnection([FromBody] OpcCredentials settings)
    {
        using var client = await uaClientFactory.CreateAsync(
            settings.OpcUaServerAddress,
            settings.OpcUaUsername ?? string.Empty,
            settings.OpcUaPassword ?? string.Empty
        );

        if (client is null)
        {
            return BadRequest("Failed to connect to the OPC UA server. Please validate the service address and credentials if applicable.");
        }
        return Ok("Successfully connected with the OPC UA server.");
    }
}