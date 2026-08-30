using ServiceDeskSystemApp.Models.Auth;

namespace ServiceDeskSystemApp.Services;

public class ProfileService : IProfileService
{
    private readonly ApiService _apiService;

    public ProfileService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<UserDto?> GetProfileAsync() 
        => await _apiService.GetAsync<UserDto>("/api/profile");

    public async Task<bool> UpdateProfileAsync(UserDto profile) 
        => await _apiService.PostAsync("/api/profile", profile); // Assuming POST or PUT is used
}
