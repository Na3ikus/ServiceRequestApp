using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskSystem.Api.Models;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Api.Services;
using System.Net.Mail;

namespace ServiceDeskSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public sealed class AdminController(
    IAdminService adminService,
    IEmailSender emailSender,
    ICurrentUserService currentUserService,
    ILogger<AdminController> logger) : ControllerBase
{
    // ───────── Tech Stacks ─────────

    [HttpGet("techstacks")]
    public async Task<IActionResult> GetAllTechStacks()
    {
        logger.LogInformation("Fetching all tech stacks");
        var techStacks = await adminService.GetAllTechStacksAsync().ConfigureAwait(false);
        return Ok(techStacks);
    }

    [HttpPost("techstacks")]
    public async Task<IActionResult> CreateTechStack([FromBody] TechStackRequest request)
    {
        logger.LogInformation("Creating tech stack: {Name}", request.Name);
        var entity = new TechStack { Name = request.Name, Type = request.Type };
        var created = await adminService.CreateTechStackAsync(entity).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetAllTechStacks), new { id = created.Id }, created);
    }

    [HttpPut("techstacks/{id:int}")]
    public async Task<IActionResult> UpdateTechStack(int id, [FromBody] TechStackRequest request)
    {
        logger.LogInformation("Updating tech stack {TechStackId}", id);
        var entity = new TechStack { Id = id, Name = request.Name, Type = request.Type };
        var success = await adminService.UpdateTechStackAsync(entity).ConfigureAwait(false);

        if (!success)
        {
            return NotFound(new ApiErrorResponse(404, $"TechStack with ID {id} not found."));
        }

        return NoContent();
    }

    [HttpDelete("techstacks/{id:int}")]
    public async Task<IActionResult> DeleteTechStack(int id)
    {
        logger.LogInformation("Deleting tech stack {TechStackId}", id);
        var success = await adminService.DeleteTechStackAsync(id).ConfigureAwait(false);

        if (!success)
        {
            return BadRequest(new ApiErrorResponse(400, "Cannot delete: tech stack not found or has associated products."));
        }

        return NoContent();
    }

    // ───────── Products ─────────

    [HttpGet("products")]
    public async Task<IActionResult> GetAllProducts()
    {
        logger.LogInformation("Fetching all products");
        var products = await adminService.GetAllProductsAsync().ConfigureAwait(false);
        return Ok(products);
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequest request)
    {
        logger.LogInformation("Creating product: {Name}", request.Name);
        var entity = new Product
        {
            Name = request.Name,
            Description = request.Description,
            CurrentVersion = request.CurrentVersion,
            TechStackId = request.TechStackId,
        };
        var created = await adminService.CreateProductAsync(entity).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetAllProducts), new { id = created.Id }, created);
    }

    [HttpPut("products/{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequest request)
    {
        logger.LogInformation("Updating product {ProductId}", id);
        var entity = new Product
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            CurrentVersion = request.CurrentVersion,
            TechStackId = request.TechStackId,
        };
        var success = await adminService.UpdateProductAsync(entity).ConfigureAwait(false);

        if (!success)
        {
            return NotFound(new ApiErrorResponse(404, $"Product with ID {id} not found."));
        }

        return NoContent();
    }

    [HttpDelete("products/{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        logger.LogInformation("Deleting product {ProductId}", id);
        var success = await adminService.DeleteProductAsync(id).ConfigureAwait(false);

        if (!success)
        {
            return BadRequest(new ApiErrorResponse(400, "Cannot delete: product not found or has associated tickets."));
        }

        return NoContent();
    }

    // ───────── Users ─────────

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        logger.LogInformation("Fetching all users");
        var users = await adminService.GetAllUsersAsync().ConfigureAwait(false);
        return Ok(users);
    }

    [HttpPut("users/{userId:int}/role")]
    public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UserRole newRole)
    {
        // Prevent an admin from removing their own admin role (self-demotion lockout)
        if (currentUserService.UserId == userId)
        {
            return BadRequest(new ApiErrorResponse(400, "You cannot change your own role."));
        }

        logger.LogInformation("Updating role of user {UserId} to {Role}", userId, newRole);
        var success = await adminService.UpdateUserRoleAsync(userId, newRole).ConfigureAwait(false);

        if (!success)
        {
            return NotFound(new ApiErrorResponse(404, $"User with ID {userId} not found."));
        }

        return NoContent();
    }

    [HttpPut("users/{userId:int}/toggle")]
    public async Task<IActionResult> ToggleUserActive(int userId)
    {
        // Prevent an admin from deactivating their own account
        if (currentUserService.UserId == userId)
        {
            return BadRequest(new ApiErrorResponse(400, "You cannot deactivate your own account."));
        }

        logger.LogInformation("Toggling active status of user {UserId}", userId);
        var success = await adminService.ToggleUserActiveStatusAsync(userId).ConfigureAwait(false);

        if (!success)
        {
            return NotFound(new ApiErrorResponse(404, $"User with ID {userId} not found."));
        }

        return NoContent();
    }

    [HttpDelete("users/{userId:int}")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        logger.LogInformation("Deleting user {UserId}", userId);
        var success = await adminService.DeleteUserAsync(userId).ConfigureAwait(false);

        if (!success)
        {
            return BadRequest(new ApiErrorResponse(400, "Cannot delete: user not found or has associated tickets."));
        }

        return NoContent();
    }

    [HttpGet("smtp/check")]
    public async Task<IActionResult> CheckSmtp(CancellationToken cancellationToken)
    {
        var (isSuccess, message) = await emailSender.CheckConnectionAsync(cancellationToken).ConfigureAwait(false);

        if (!isSuccess)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ApiErrorResponse(503, message));
        }

        return Ok(new { IsAvailable = true, Message = message });
    }

    [HttpPost("smtp/test-email")]
    public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ToEmail) || !MailAddress.TryCreate(request.ToEmail, out _))
        {
            return BadRequest(new ApiErrorResponse(400, "A valid recipient email is required."));
        }

        var subject = string.IsNullOrWhiteSpace(request.Subject)
            ? "ServiceDesk SMTP test"
            : request.Subject.Trim();

        var utcNow = DateTime.UtcNow;
        var textBody = $"SMTP test email from ServiceDeskSystem at {utcNow:O}.";
        var htmlBody = $"<p><strong>SMTP test email</strong> from ServiceDeskSystem.</p><p>UTC: {utcNow:O}</p>";

        await emailSender.SendAsync(request.ToEmail.Trim(), subject, htmlBody, textBody, cancellationToken).ConfigureAwait(false);
        return Ok(new { Sent = true });
    }
}

