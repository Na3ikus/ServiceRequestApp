using ServiceDeskSystem.Infrastructure.Data.Repository;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Infrastructure.Data;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using System.Security.Cryptography;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;

namespace ServiceDeskSystem.Application.Services.Auth;

public sealed class AuthService(
    IDbContextFactory<BugTrackerDbContext> contextFactory,
    ProtectedSessionStorage? sessionStorage = null) : IAuthService
{
    private const int Pbkdf2Iterations = 100_000;
    private const int MinPasswordLength = 8;

    private bool initialized;

    public event EventHandler? AuthStateChanged;

    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => this.CurrentUser is not null;

    public async Task EnsureRestoredAsync()
    {
        if (this.initialized)
        {
            return;
        }

        this.initialized = true;

        try
        {
            if (sessionStorage is null)
            {
                return;
            }

            var stored = await sessionStorage.GetAsync<int>("authUserId").ConfigureAwait(false);
            if (stored.Success && stored.Value > 0)
            {
                await using var repo = new RepositoryFacade(contextFactory);
                var user = await repo.Users.GetByIdAsync(stored.Value).ConfigureAwait(false);

                if (user is not null && user.IsActive)
                {
                    this.CurrentUser = user;
                    this.AuthStateChanged?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    await sessionStorage.DeleteAsync("authUserId").ConfigureAwait(false);
                }
            }
        }
        catch
        {
            // It's safe to ignore exceptions here, as failure to restore session does not affect application logic.
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Username and password are required.");
        }

        await using var repo = new RepositoryFacade(contextFactory);
        var user = await repo.Users.GetByLoginAsync(username).ConfigureAwait(false);

        if (user is null)
        {
            return (false, "Invalid username or password.");
        }

        if (!user.IsActive)
        {
            return (false, "Account is deactivated. Please contact administrator.");
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            return (false, "Invalid username or password.");
        }

        this.CurrentUser = user;
        await this.SaveToSessionAsync(user).ConfigureAwait(false);
        this.AuthStateChanged?.Invoke(this, EventArgs.Empty);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterClientAsync(string username, string password, string firstName, string lastName, string? email)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Username and password are required.");
        }

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return (false, "First name and last name are required.");
        }

        if (password.Length < MinPasswordLength)
        {
            return (false, $"Password must be at least {MinPasswordLength} characters long.");
        }

        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            var existingUser = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Login == username)
                .ConfigureAwait(false);

            if (existingUser is not null)
            {
                return (false, "Username already exists.");
            }

            ContactType? emailContactType = null;
            if (!string.IsNullOrWhiteSpace(email))
            {
                emailContactType = await dbContext.ContactTypes
                    .FirstOrDefaultAsync(ct => ct.Name == "Email")
                    .ConfigureAwait(false);

                if (emailContactType is not null)
                {
                    var existingEmail = await dbContext.ContactInfos
                        .AnyAsync(ci => ci.ContactTypeId == emailContactType.Id && ci.Value == email)
                        .ConfigureAwait(false);

                    if (existingEmail)
                    {
                        return (false, "Email address is already registered.");
                    }
                }
            }

            var person = new Person
            {
                FirstName = firstName,
                LastName = lastName,
            };

            if (!string.IsNullOrWhiteSpace(email) && emailContactType is not null)
            {
                person.ContactInfos.Add(new ContactInfo
                {
                    ContactTypeId = emailContactType.Id,
                    Value = email,
                });
            }

            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            var user = new User
            {
                Login = username,
                PasswordHash = ComputeSecureHash(password),
                Role = "User",
                PersonId = person.Id,
                IsActive = true,
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return (true, null);
        }
    }

    public async Task LogoutAsync()
    {
        this.CurrentUser = null;
        try
        {
            if (sessionStorage is not null)
            {
                await sessionStorage.DeleteAsync("authUserId").ConfigureAwait(false);
            }
        }
        catch
        {
            // It's safe to ignore exceptions here, as failure to delete the session does not affect logout logic.
        }

        this.AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var storedHashBytes = Convert.FromBase64String(parts[1]);

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            32);

        return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
    }

    private static string ComputeSecureHash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            32);

        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    private async Task SaveToSessionAsync(User user)
    {
        try
        {
            if (sessionStorage is not null)
            {
                await sessionStorage.SetAsync("authUserId", user.Id).ConfigureAwait(false);
            }
        }
        catch
        {
            // It's safe to ignore exceptions here, as failure to save the session does not affect authentication logic.
        }
    }
}

