using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Services.Auth;

internal sealed class SimpleAuthService(
    IDbContextFactory<BugTrackerDbContext> contextFactory,
    ProtectedSessionStorage sessionStorage) : IAuthService
{
    public event EventHandler? AuthStateChanged;

    public User? CurrentUser { get; private set; }
    private bool initialized;

    public bool IsAuthenticated => this.CurrentUser is not null;

    public async Task EnsureRestoredAsync()
    {
        if (initialized)
            return;

        initialized = true;

        try
        {
            var stored = await sessionStorage.GetAsync<int>("authUserId");
            if (stored.Success && stored.Value > 0)
            {
                var dbContext = await contextFactory.CreateDbContextAsync();
                await using (dbContext)
                {
                    var user = await dbContext.Users
                        .Include(u => u.Person)
                        .FirstOrDefaultAsync(u => u.Id == stored.Value);

                    if (user is not null)
                    {
                        this.CurrentUser = user;
                        this.AuthStateChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }
        catch
        {
            // ProtectedSessionStorage може кинути виключення до першого рендеру - ігноруємо
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

            if (!VerifyPassword(password, user.PasswordHash))
            {
                return (false, "Invalid username or password.");
            }

            this.CurrentUser = user;
            await SaveToSessionAsync(user).ConfigureAwait(false);
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
                Role = "Client",
                PersonId = person.Id,
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
            await sessionStorage.DeleteAsync("authUserId");
        }
        catch { }
        this.AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task SaveToSessionAsync(User user)
    {
        try
        {
            await sessionStorage.SetAsync("authUserId", user.Id);
        }
        catch { }
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
