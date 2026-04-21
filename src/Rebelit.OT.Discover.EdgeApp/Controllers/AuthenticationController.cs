using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Authentication;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(
    IxonAuthentication ixonAuthentication
) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> IxonLoginAsync([FromBody] Authentication model)
    {
        var token = await ixonAuthentication.BearerTokenGenerator(
            model.Username,
            model.Password,
            model.ApplicationID,
            model.OtpCode
        );

        return Ok(new { bearerToken = token });
    }
}