using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.Auth;

internal interface IAuthService
{
    event EventHandler? AuthStateChanged;

    User? CurrentUser { get; }

    bool IsAuthenticated { get; }

    Task EnsureRestoredAsync();

    Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password);

    Task<(bool Success, string? ErrorMessage)> RegisterClientAsync(string username, string password, string firstName, string lastName, string? email);

    void Logout();
}
