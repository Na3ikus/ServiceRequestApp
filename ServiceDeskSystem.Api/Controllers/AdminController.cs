using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskSystem.Api.Models;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Admin")] // TODO: ввімкнути після налаштування JWT
public sealed class AdminController(
    IAdminService adminService,
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
    public async Task<IActionResult> CreateTechStack([FromBody] TechStack techStack)
    {
        logger.LogInformation("Creating tech stack: {Name}", techStack.Name);
        var created = await adminService.CreateTechStackAsync(techStack).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetAllTechStacks), new { id = created.Id }, created);
    }

    [HttpPut("techstacks/{id:int}")]
    public async Task<IActionResult> UpdateTechStack(int id, [FromBody] TechStack techStack)
    {
        techStack.Id = id;
        logger.LogInformation("Updating tech stack {TechStackId}", id);
        var success = await adminService.UpdateTechStackAsync(techStack).ConfigureAwait(false);

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
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        logger.LogInformation("Creating product: {Name}", product.Name);
        var created = await adminService.CreateProductAsync(product).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetAllProducts), new { id = created.Id }, created);
    }

    [HttpPut("products/{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
    {
        product.Id = id;
        logger.LogInformation("Updating product {ProductId}", id);
        var success = await adminService.UpdateProductAsync(product).ConfigureAwait(false);

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
    public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] string newRole)
    {
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
}
