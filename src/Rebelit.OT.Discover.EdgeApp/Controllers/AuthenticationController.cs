using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Dto;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    [HttpPost]
    [Route("validate")]
    public IActionResult ValidateCredentials([FromBody] CredentialsDto credentials)
    {        // This is a placeholder for the actual authentication logic.
        // In a real application, you would validate the user's credentials and generate a token or session.
        return Ok(new { message = "Authentication successful." });
    }
}