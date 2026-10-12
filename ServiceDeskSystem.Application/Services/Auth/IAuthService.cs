using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.Auth;

public interface IAuthService
{
    event EventHandler? AuthStateChanged;

    User? CurrentUser { get; }

    bool IsAuthenticated { get; }

    Task EnsureRestoredAsync();

    Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password, string? ipAddress = null);

    Task<(bool Success, string? ErrorMessage)> RegisterClientAsync(string username, string password, string firstName, string lastName, string? email);

    /// <summary>
    /// Applies a saved profile name to the signed-in user so the shell (avatar, name) updates without a reload.
    /// </summary>
    void UpdateCurrentPerson(string firstName, string lastName, string? middleName);

    Task LogoutAsync();
}

