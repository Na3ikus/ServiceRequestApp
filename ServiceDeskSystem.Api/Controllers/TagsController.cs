using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskSystem.Api.Models;
using ServiceDeskSystem.Api.Services;
using ServiceDeskSystem.Application.Services.Tags;

namespace ServiceDeskSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TagsController(
    ITagService tagService,
    ICurrentUserService currentUserService,
    ILogger<TagsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tags = await tagService.GetAllTagsAsync().ConfigureAwait(false);
        var response = tags.Select(t => new TagResponse(t.Id, t.Name, t.Color, t.Tickets?.Count ?? 0));
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tag = await tagService.GetTagByIdAsync(id).ConfigureAwait(false);
        if (tag is null)
        {
            return NotFound(new ApiErrorResponse(404, $"Tag with ID {id} not found."));
        }

        return Ok(new TagResponse(tag.Id, tag.Name, tag.Color, tag.Tickets?.Count ?? 0));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ApiErrorResponse(400, "Tag name is required."));
        }

        var created = await tagService.CreateTagAsync(request.Name, request.Color, currentUserService.UserId).ConfigureAwait(false);
        logger.LogInformation("Tag {TagName} created by user {UserId}", created.Name, currentUserService.UserId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new TagResponse(created.Id, created.Name, created.Color, 0));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ApiErrorResponse(400, "Tag name is required."));
        }

        var updated = await tagService.UpdateTagAsync(id, request.Name, request.Color, currentUserService.UserId).ConfigureAwait(false);
        if (updated is null)
        {
            return NotFound(new ApiErrorResponse(404, $"Tag with ID {id} not found."));
        }

        logger.LogInformation("Tag {TagId} updated by user {UserId}", id, currentUserService.UserId);
        return Ok(new TagResponse(updated.Id, updated.Name, updated.Color, updated.Tickets?.Count ?? 0));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await tagService.DeleteTagAsync(id, currentUserService.UserId).ConfigureAwait(false);
        if (!success)
        {
            return NotFound(new ApiErrorResponse(404, $"Tag with ID {id} not found."));
        }

        logger.LogInformation("Tag {TagId} deleted by user {UserId}", id, currentUserService.UserId);
        return NoContent();
    }

    [HttpPost("tickets/{ticketId:int}/assign/{tagId:int}")]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> AssignTag(int ticketId, int tagId)
    {
        var success = await tagService.AssignTagToTicketAsync(ticketId, tagId, currentUserService.UserId).ConfigureAwait(false);
        if (!success)
        {
            return NotFound(new ApiErrorResponse(404, $"Ticket {ticketId} or Tag {tagId} not found."));
        }

        return Ok(new { message = $"Tag {tagId} successfully assigned to ticket {ticketId}." });
    }

    [HttpDelete("tickets/{ticketId:int}/remove/{tagId:int}")]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> RemoveTag(int ticketId, int tagId)
    {
        var success = await tagService.RemoveTagFromTicketAsync(ticketId, tagId, currentUserService.UserId).ConfigureAwait(false);
        if (!success)
        {
            return NotFound(new ApiErrorResponse(404, $"Ticket {ticketId} not found."));
        }

        return Ok(new { message = $"Tag {tagId} successfully removed from ticket {ticketId}." });
    }
}
