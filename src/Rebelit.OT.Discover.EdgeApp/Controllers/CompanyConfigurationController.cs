using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Services;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[Route("api/[controller]")]
public class CompanyConfigurationController(
    ICompanyConfigurationService service
) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetConfiguration()
    {
        var result = await service.GetConfigurationAsync();
        if(result == null)
        {
            return BadRequest();
        }
        return Ok(result);
    }
}