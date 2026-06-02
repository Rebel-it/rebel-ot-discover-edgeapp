using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.API.Models;
using Rebelit.OT.Discover.EdgeApp.API.Services;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

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

    [HttpGet("prefilled")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPrefilledTagsAsync()
    {
        var tags = await _tagService.GetPrefilledTagsAsync();
        return Ok(tags); 
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTagAsync([FromBody] Tag model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return BadRequest(new { message = "Tag name is required." });
        }

        var createdTag = await _tagService.CreateTagAsync(model);
        return Ok(createdTag);
    }

    [HttpPost("CreateTags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTagsAsync([FromBody] List<Tag> model)
    {
        var createdTags = await _tagService.CreateTagsAsync(model); 

        return Ok(createdTags);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTagAsync([FromBody] UpdateTagRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.PublicId))
        {
            return BadRequest(new { message = "Tag public ID is required." });
        }

        var updatedTag = await _tagService.UpdateTagAsync(model);
        return Ok(updatedTag);
    }
}