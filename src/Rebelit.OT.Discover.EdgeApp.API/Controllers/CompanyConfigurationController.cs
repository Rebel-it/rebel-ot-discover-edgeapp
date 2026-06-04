using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.API.Services;

namespace Rebelit.OT.Discover.EdgeApp.API.Controllers;

public class CompanyConfigurationController(
    IIxonCompanyConfigurationService service
) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetConfiguration()
    {
        var result = await service.GetConfigurationAsync();
        if (result.Success)
        {
            return Ok(result.Data);
        }
        return BadRequest(result.ErrorMessage);
    }
}