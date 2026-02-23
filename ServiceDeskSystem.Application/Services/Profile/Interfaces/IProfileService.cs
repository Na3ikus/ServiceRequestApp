using ServiceDeskSystem.Application.Services.Profile.Models;

namespace ServiceDeskSystem.Application.Services.Profile.Interfaces;

public interface IProfileService
{
    Task<UserProfileDto?> GetProfileAsync(int userId);

    Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(int userId, UpdateProfileRequest request);

    Task<List<ContactTypeDto>> GetContactTypesAsync();
}

public record ContactTypeDto(int Id, string Name);
