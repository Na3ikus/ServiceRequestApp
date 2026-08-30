using ServiceDeskSystemApp.Models.Auth;

namespace ServiceDeskSystemApp.Services;

public interface IProfileService
{
    Task<UserDto?> GetProfileAsync();
    Task<bool> UpdateProfileAsync(UserDto profile);
}
