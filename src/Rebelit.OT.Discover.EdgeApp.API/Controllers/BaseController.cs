using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.API.Filters;

namespace Rebelit.OT.Discover.EdgeApp.API.Controllers;

[ApiController]
[ServiceFilter(typeof(AuthenticationFilter))]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
}