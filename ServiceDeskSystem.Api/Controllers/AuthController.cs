using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskSystem.Api.Models;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace ServiceDeskSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    IAuthService authService,
    IJwtTokenService jwtTokenService,
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
        var token = jwtTokenService.GenerateToken(user);

        // Append token as HttpOnly Cookie
        Response.Cookies.Append("AccessToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,   // Ideally true in prod, but keeping it true helps modern browsers
            SameSite = SameSiteMode.Strict,
            Expires = null   // Session cookie: cleared when browser closes
        });

        logger.LogInformation("User {Username} logged in successfully", request.Username);

        return Ok(new { Message = "Login successful.", User = MapToDto(user) });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AccessToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return Ok(new { Message = "Logout successful." });
    }

    [HttpGet("validate")]
    [Authorize]
    public IActionResult ValidateToken()
    {
        // If the request reaches here, the token is valid (handled by [Authorize] + JWT middleware).
        return Ok(new { IsValid = true, Message = "Session is active." });
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
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(
        [FromServices] Microsoft.EntityFrameworkCore.IDbContextFactory<ServiceDeskSystem.Infrastructure.Data.BugTrackerDbContext> contextFactory,
        [FromServices] ICurrentUserService currentUserService)
    {
        var userId = currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized(new ApiErrorResponse(401, "User not authenticated by token."));
        }

        await using var repo = new ServiceDeskSystem.Infrastructure.Data.Repository.RepositoryFacade(contextFactory);
        var user = await repo.Users.GetByIdAsync(userId.Value).ConfigureAwait(false);
        if (user is null)
        {
            return Unauthorized(new ApiErrorResponse(401, "User not found."));
        }

        return Ok(MapToDto(user));
    }

    private static UserDto MapToDto(User user) =>
        new(user.Id, user.Login, user.Role, user.IsActive,
            user.Person?.FirstName, user.Person?.LastName);
}

