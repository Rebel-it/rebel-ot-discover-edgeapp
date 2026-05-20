using Microsoft.AspNetCore.Mvc;
using Opc.Ua.Server;
using Rebelit.OT.Discover.EdgeApp.API.Resolvers;
using Rebelit.OT.Discover.EdgeApp.API.Services;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.API.Controllers;

public class IxonSettingsController(
    IDataSourceResolver dataSourceResolver,
    IIxonSettingService ixonSettingService
) : BaseController
{
    [HttpPost("datasource")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveDataSourceId([FromBody] SaveDataSourceRequest saveDataSourceRequest)
    {
        var result = await dataSourceResolver.ResolveAsync(saveDataSourceRequest.DataSourceName);

        if (string.IsNullOrEmpty(result))
        {
            return BadRequest(new { message = "Failed to resolve data source ID." });
        }

        return Ok(result);
    }

    [HttpPost("pushConfiguration")]
    public async Task<IActionResult> PushConfiguration()
    {
        var result = await ixonSettingService.PushDeviceConfig();
        if (string.IsNullOrEmpty(result))
        {
            return BadRequest(new { message = "Failed to push device configuration." });
        }

        return Ok(result);
    }
}