using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Services;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VariableController(IVariableService variableService) : BaseController
{
    private readonly IVariableService _variableService = variableService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetVariablesAsync(CancellationToken cancellationToken)
    {
        var variables = await _variableService.GetVariablesAsync(cancellationToken);
        return Ok(variables);
    }
}
