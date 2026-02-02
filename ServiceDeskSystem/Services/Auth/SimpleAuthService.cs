using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services.Auth;

internal sealed class SimpleAuthService(
    IDbContextFactory<BugTrackerDbContext> contextFactory,
    ProtectedSessionStorage sessionStorage) : IAuthService
{
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
            var stored = await sessionStorage.GetAsync<int>("authUserId").ConfigureAwait(false);
            if (stored.Success && stored.Value > 0)
            {
                var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
                await using (dbContext.ConfigureAwait(false))
                {
                    var user = await dbContext.Users
                        .Include(u => u.Person)
                        .FirstOrDefaultAsync(u => u.Id == stored.Value)
                        .ConfigureAwait(false);

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
        }
        catch
        {
            // ProtectedSessionStorage may throw an exception before the first render.
            // This can be safely ignored as it does not affect authentication state restoration.
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Username and password are required.");
        }

        var dbContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            var user = await dbContext.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Login == username)
                .ConfigureAwait(false);

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

        if (password.Length < 6)
        {
            return (false, "Password must be at least 6 characters long.");
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

            if (!string.IsNullOrWhiteSpace(email))
            {
                var emailContactType = await dbContext.ContactTypes
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

            if (!string.IsNullOrWhiteSpace(email))
            {
                var emailContactType = await dbContext.ContactTypes
                    .FirstOrDefaultAsync(ct => ct.Name == "Email")
                    .ConfigureAwait(false);

                if (emailContactType is not null)
                {
                    person.ContactInfos.Add(new ContactInfo
                    {
                        ContactTypeId = emailContactType.Id,
                        Value = email,
                    });
                }
            }

            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            var user = new User
            {
                Login = username,
                PasswordHash = ComputeSimpleHash(password),
                Role = "User",
                PersonId = person.Id,
                IsActive = true,
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return (true, null);
        }
    }

    public async void Logout()
    {
        this.CurrentUser = null;
        try
        {
            await sessionStorage.DeleteAsync("authUserId").ConfigureAwait(false);
        }
        catch
        {
            // It's safe to ignore exceptions here, as failure to delete the session does not affect logout logic.
        }

        this.AuthStateChanged?.Invoke(this, EventArgs.Empty);
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

    private async Task SaveToSessionAsync(User user)
    {
        try
        {
            await sessionStorage.SetAsync("authUserId", user.Id).ConfigureAwait(false);
        }
        catch
        {
            // It's safe to ignore exceptions here, as failure to save the session does not affect authentication logic.
        }
    }
}
