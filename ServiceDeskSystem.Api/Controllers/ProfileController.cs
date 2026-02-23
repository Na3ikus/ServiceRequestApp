using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskSystem.Api.Models; // For ApiErrorResponse if needed
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Profile.Interfaces;
using ServiceDeskSystem.Application.Services.Profile.Models;

namespace ServiceDeskSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class ProfileController(
    IProfileService profileService,
    IAuthService authService,
    ILogger<ProfileController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var user = authService.CurrentUser;
        if (user is null)
            return Unauthorized(new ApiErrorResponse(401, "User not authenticated."));

        var profile = await profileService.GetProfileAsync(user.Id).ConfigureAwait(false);
        if (profile is null)
            return NotFound(new ApiErrorResponse(404, "Profile not found."));

        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = authService.CurrentUser;
        if (user is null)
            return Unauthorized(new ApiErrorResponse(401, "User not authenticated."));

        var (success, errorMessage) = await profileService.UpdateProfileAsync(user.Id, request).ConfigureAwait(false);

        if (!success)
        {
            logger.LogWarning("Profile update failed for user {UserId}: {Error}", user.Id, errorMessage);
            return BadRequest(new ApiErrorResponse(400, errorMessage ?? "Failed to update profile."));
        }

        return Ok(new { Message = "Profile updated successfully." });
    }

    [HttpGet("contact-types")]
    public async Task<IActionResult> GetContactTypes()
    {
        var types = await profileService.GetContactTypesAsync().ConfigureAwait(false);
        return Ok(types);
    }
}
