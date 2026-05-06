using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Dto;
using Rebelit.OT.Discover.EdgeApp.Services;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[Route("api/[controller]")]
public class CompanyConfigurationController(
    ICompanyConfigurationService service,
    IIxonAuthenticationContext authenticationContext
) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetConfiguration()
    {
        var serviceAccount = new ServiceAccountDto
        {
            ApiApplicationId = authenticationContext.IxonCredentials.ApplicationId,
            AccessToken = authenticationContext.IxonCredentials.AccessToken
        };

        var result = await service.GetConfigurationAsync(serviceAccount);
        if(result == null)
        {
            return BadRequest();
        }
        return Ok(result);
    }
}