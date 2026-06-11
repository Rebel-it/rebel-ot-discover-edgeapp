using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.API.Resolvers;
using Rebelit.OT.Discover.EdgeApp.API.Services;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.API.Controllers;

public class IxonSettingsController(
    IDataSourceResolver dataSourceResolver,
    IIxonSettingService ixonSettingService
) : BaseController
{
    [HttpPost("DataSource")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostNewSource([FromBody] SaveDataSourceRequest saveDataSourceRequest)
    {
        var result = await dataSourceResolver.ResolveAsync(saveDataSourceRequest.DataSourceName);

        if (string.IsNullOrEmpty(result))
        {
            return BadRequest("Failed to resolve data source ID.");
        }

        return Ok(result);
    }

    [HttpPost("PushConfiguration")]
    public async Task<IActionResult> PushDeviceConfiguration()
    {
        var result = await ixonSettingService.PushDeviceConfigAsync();
        if (result != "success")
        {
            return BadRequest("Failed to push device configuration.");
        }

        return Ok(result);
    }
}