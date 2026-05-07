using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Services;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

public class CompanyConfigurationController(
    ICompanyConfigurationService service
) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetConfiguration()
    {
        var result = await service.GetConfigurationAsync();
        return Ok(result);
    }
}