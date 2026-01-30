using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services;

public interface IAuthService
{
    User? CurrentUser { get; }

    bool IsAuthenticated { get; }

    Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password);

    void Logout();

    event Action? OnAuthStateChanged;
}
