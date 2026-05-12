using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.API.Models;
using Rebelit.OT.Discover.EdgeApp.API.Services;

namespace Rebelit.OT.Discover.EdgeApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController(ITagService tagService) : BaseController
{
    private readonly ITagService _tagService = tagService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTagsAsync()
    {
        var tags = await _tagService.GetTagsAsync();
        return Ok(tags);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTagAsync([FromBody] CreateTagRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            return BadRequest(new { message = "Tag name is required." });

        var createdTag = await _tagService.CreateTagAsync(model);
        return Ok(createdTag);
    }
}
