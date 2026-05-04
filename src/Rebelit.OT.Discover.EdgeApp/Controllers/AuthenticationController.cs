using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Authentication;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Services;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(
    IxonAuthentication ixonAuthentication,
    IAppSettingsManager settingsManager
) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> IxonLoginAsync([FromBody] Authentication model)
    {
        try
        {
            var token = await ixonAuthentication.BearerTokenGenerator(
                model.Username,
                model.Password,
                model.ApplicationID,
                model.OtpCode
            );

            settingsManager.Save(new Dictionary<string, string?>
            {
                ["IXON_BearerToken"] = token,
                ["IXON_ApplicationID"] = model.ApplicationID
            });

            //Get company id and save it to settings
            var companyId = await ixonAuthentication.CompanyIdAsync(token, model.ApplicationID);

            settingsManager.Save(new Dictionary<string, string?>
            {
                ["IXON_CompanyID"] = companyId
            });

            return Ok(new { bearerToken = token });
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            ex.StatusCode == System.Net.HttpStatusCode.BadRequest
        )
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }
    }
}