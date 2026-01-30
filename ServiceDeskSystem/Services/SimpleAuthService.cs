using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services;

internal sealed class SimpleAuthService(IDbContextFactory<BugTrackerDbContext> contextFactory) : IAuthService
{
    public User? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public event EventHandler? AuthStateChanged;

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Username and password are required.");
        }

        await using var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        var user = await dbContext.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Login == username)
            .ConfigureAwait(false);

        if (user is null)
        {
            return (false, "Invalid username or password.");
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            return (false, "Invalid username or password.");
        }

        CurrentUser = user;
        AuthStateChanged?.Invoke(this, EventArgs.Empty);

        return (true, null);
    }

    public void Logout()
    {
        CurrentUser = null;
        AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        if (password == storedHash)
        {
            return true;
        }

        var hash = ComputeSimpleHash(password);
        return hash == storedHash;
    }

    private static string ComputeSimpleHash(string input)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}
