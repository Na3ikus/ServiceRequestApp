using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services;

internal interface IAuthService
{
    User? CurrentUser { get; }

    bool IsAuthenticated { get; }

    Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password);

    Task<(bool Success, string? ErrorMessage)> RegisterClientAsync(string username, string password, string firstName, string lastName, string? email);

    void Logout();

    event EventHandler? AuthStateChanged;
}
