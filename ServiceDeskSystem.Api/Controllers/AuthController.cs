using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskSystem.Api.Models;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        logger.LogInformation("Login attempt for user: {Username}", request.Username);

        var (success, errorMessage) = await authService.LoginAsync(request.Username, request.Password).ConfigureAwait(false);

        if (!success)
        {
            logger.LogWarning("Login failed for user: {Username}. Reason: {Error}", request.Username, errorMessage);
            return Unauthorized(new ApiErrorResponse(401, errorMessage ?? "Invalid credentials."));
        }

        var user = authService.CurrentUser!;

        logger.LogInformation("User {Username} logged in successfully", request.Username);

        // TODO: Повернути JWT токен після налаштування автентифікації
        return Ok(new { Message = "Login successful.", User = MapToDto(user) });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        logger.LogInformation("Registration attempt for user: {Username}", request.Username);

        var (success, errorMessage) = await authService.RegisterClientAsync(
            request.Username, request.Password, request.FirstName, request.LastName, request.Email).ConfigureAwait(false);

        if (!success)
        {
            logger.LogWarning("Registration failed for user: {Username}. Reason: {Error}", request.Username, errorMessage);
            return BadRequest(new ApiErrorResponse(400, errorMessage ?? "Registration failed."));
        }

        logger.LogInformation("User {Username} registered successfully", request.Username);
        return Ok(new { Message = "Registration successful." });
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var user = authService.CurrentUser;
        if (user is null)
        {
            return Unauthorized(new ApiErrorResponse(401, "User not authenticated."));
        }

        return Ok(MapToDto(user));
    }

    private static UserDto MapToDto(User user) =>
        new(user.Id, user.Login, user.Role, user.IsActive,
            user.Person?.FirstName, user.Person?.LastName);
}

