using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Services;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController(ITagService tagService) : ControllerBase
{
    private readonly ITagService _tagService = tagService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTagsAsync(CancellationToken cancellationToken)
    {
        var tags = await _tagService.GetTagsAsync(cancellationToken);
        return Ok(tags);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateTag([FromBody] Tag model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return BadRequest(new { message = "Tag name is required." });
        }

        var createdTag = new { Id = new Random().Next(1000, 9999), Name = model.Name };
        return Ok(createdTag);
    }
}
