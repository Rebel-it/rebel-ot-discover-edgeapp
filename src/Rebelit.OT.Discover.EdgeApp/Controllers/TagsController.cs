using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.Models;
using Rebelit.OT.Discover.EdgeApp.Services;

namespace Rebelit.OT.Discover.EdgeApp.Controllers;

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

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTagAsync([FromBody] UpdateTagRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.PublicId))
            return BadRequest(new { message = "Tag public ID is required." });

        var updatedTag = await _tagService.UpdateTagAsync(model);
        return Ok(updatedTag);
    }
}