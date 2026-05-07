using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Filters;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[ApiController]
[ServiceFilter(typeof(AuthenticationFilter))]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
}