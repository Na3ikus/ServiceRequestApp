using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskSystem.Api.Models;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // TODO: ввімкнути після налаштування JWT
public sealed class TicketsController(
    ITicketService ticketService,
    ILogger<TicketsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Fetching all tickets");
        var tickets = await ticketService.GetAllTicketsAsync().ConfigureAwait(false);
        return Ok(tickets);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        logger.LogInformation("Fetching ticket {TicketId}", id);
        var ticket = await ticketService.GetTicketByIdAsync(id).ConfigureAwait(false);

        if (ticket is null)
        {
            return NotFound(new ApiErrorResponse(404, $"Ticket with ID {id} not found."));
        }

        return Ok(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Ticket ticket)
    {
        logger.LogInformation("Creating new ticket: {Title}", ticket.Title);
        var created = await ticketService.CreateTicketAsync(ticket).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        logger.LogInformation("Updating ticket {TicketId} status to {Status}", id, request.Status);
        var success = await ticketService.UpdateTicketStatusAsync(id, request.Status).ConfigureAwait(false);

        if (!success)
        {
            return NotFound(new ApiErrorResponse(404, $"Ticket with ID {id} not found."));
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        logger.LogInformation("Deleting ticket {TicketId}", id);
        var success = await ticketService.DeleteTicketAsync(id).ConfigureAwait(false);

        if (!success)
        {
            return NotFound(new ApiErrorResponse(404, $"Ticket with ID {id} not found."));
        }

        return NoContent();
    }

    [HttpPut("{id:int}/assign")]
    public async Task<IActionResult> AssignDeveloper(int id, [FromBody] AssignDeveloperRequest request)
    {
        logger.LogInformation("Assigning developer {DeveloperId} to ticket {TicketId}", request.DeveloperId, id);
        var success = await ticketService.AssignDeveloperAsync(id, request.DeveloperId).ConfigureAwait(false);

        if (!success)
        {
            return NotFound(new ApiErrorResponse(404, $"Ticket with ID {id} not found."));
        }

        return NoContent();
    }

    [HttpPut("{id:int}/unassign")]
    public async Task<IActionResult> UnassignDeveloper(int id)
    {
        logger.LogInformation("Unassigning developer from ticket {TicketId}", id);
        var success = await ticketService.UnassignDeveloperAsync(id).ConfigureAwait(false);

        if (!success)
        {
            return NotFound(new ApiErrorResponse(404, $"Ticket with ID {id} not found."));
        }

        return NoContent();
    }

    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] CreateCommentRequest request)
    {
        logger.LogInformation("Adding comment to ticket {TicketId}", id);
        var comment = new Comment
        {
            TicketId = id,
            AuthorId = request.AuthorId,
            Message = request.Message,
        };

        var created = await ticketService.AddCommentAsync(comment).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetById), new { id }, created);
    }

    [HttpPut("comments/{commentId:int}")]
    public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentRequest request)
    {
        logger.LogInformation("Updating comment {CommentId}", commentId);
        var updated = await ticketService.UpdateCommentAsync(commentId, request.Message).ConfigureAwait(false);

        if (updated is null)
        {
            return NotFound(new ApiErrorResponse(404, $"Comment with ID {commentId} not found."));
        }

        return Ok(updated);
    }

    [HttpDelete("comments/{commentId:int}")]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        logger.LogInformation("Deleting comment {CommentId}", commentId);
        var success = await ticketService.DeleteCommentAsync(commentId).ConfigureAwait(false);

        if (!success)
        {
            return NotFound(new ApiErrorResponse(404, $"Comment with ID {commentId} not found."));
        }

        return NoContent();
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        logger.LogInformation("Fetching ticket statistics");
        var total = await ticketService.GetTotalTicketsCountAsync().ConfigureAwait(false);
        var open = await ticketService.GetOpenTicketsCountAsync().ConfigureAwait(false);
        var critical = await ticketService.GetCriticalTicketsCountAsync().ConfigureAwait(false);

        return Ok(new TicketStatsDto(total, open, critical));
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetUserTickets(int userId)
    {
        logger.LogInformation("Fetching tickets for user {UserId}", userId);
        var tickets = await ticketService.GetUserTicketsAsync(userId).ConfigureAwait(false);
        return Ok(tickets);
    }

    [HttpGet("developer/{developerId:int}")]
    public async Task<IActionResult> GetDeveloperTickets(int developerId)
    {
        logger.LogInformation("Fetching tickets for developer {DeveloperId}", developerId);
        var tickets = await ticketService.GetDeveloperTicketsAsync(developerId).ConfigureAwait(false);
        return Ok(tickets);
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
        logger.LogInformation("Fetching products list");
        var products = await ticketService.GetProductsAsync().ConfigureAwait(false);
        return Ok(products);
    }
}
