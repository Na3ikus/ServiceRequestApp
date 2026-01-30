using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services;

public class SimpleAuthService(IDbContextFactory<BugTrackerDbContext> contextFactory) : IAuthService
{
    public User? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public event Action? OnAuthStateChanged;

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Username and password are required.");
        }

        await using var dbContext = await contextFactory.CreateDbContextAsync();
        var user = await dbContext.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Login == username);

        if (user is null)
        {
            return (false, "Invalid username or password.");
        }

        // Simple password check (in production, use proper hashing like BCrypt)
        // For this demo, we compare directly or check if PasswordHash matches
        if (!VerifyPassword(password, user.PasswordHash))
        {
            return (false, "Invalid username or password.");
        }

        CurrentUser = user;
        OnAuthStateChanged?.Invoke();

        return (true, null);
    }

    public void Logout()
    {
        CurrentUser = null;
        OnAuthStateChanged?.Invoke();
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        // Simple comparison for demo purposes
        // In production, use BCrypt.Net-Next or similar:
        // return BCrypt.Net.BCrypt.Verify(password, storedHash);
        
        // Option 1: Plain text comparison (for testing)
        if (password == storedHash)
            return true;

        // Option 2: Simple hash comparison (SHA256)
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
