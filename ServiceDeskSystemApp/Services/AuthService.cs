using ServiceDeskSystemApp.Models.Auth;

namespace ServiceDeskSystemApp.Services;

public class AuthService : IAuthService
{
    private readonly ApiService _apiService;
    private string? _token;

    public AuthService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public Task<UserDto?> GetCurrentUserAsync() => throw new NotImplementedException();

    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        var request = new LoginRequest { Username = username, Password = password };
        var response = await _apiService.PostAsync<LoginRequest, LoginResponse>("/api/auth/login", request);
        
        if (response != null && !string.IsNullOrEmpty(response.Token))
        {
            _token = response.Token;
            await _apiService.SetAuthTokenAsync(_token);
        }

        return response;
    }

    public async Task LogoutAsync()
    {
        _token = null;
        await _apiService.ClearAuthTokenAsync();
    }

    public async Task<bool> RegisterAsync(RegisterRequest request) 
        => await _apiService.PostAsync("/api/auth/register", request);
}
