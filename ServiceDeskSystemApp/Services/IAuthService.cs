using ServiceDeskSystemApp.Models.Auth;

namespace ServiceDeskSystemApp.Services;

public interface IAuthService
{
    bool IsAuthenticated { get; }
    Task<LoginResponse?> LoginAsync(string username, string password);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
}
