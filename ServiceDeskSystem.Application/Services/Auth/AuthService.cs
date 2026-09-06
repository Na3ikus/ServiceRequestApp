using System;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Application.Services.Auth;

public sealed class AuthService(
    IRepositoryFacadeFactory repositoryFacadeFactory,
    ProtectedSessionStorage? sessionStorage = null,
    IAuditService? auditService = null,
    IBruteForceProtectionService? bruteForceService = null) : IAuthService
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
                await using var repo = repositoryFacadeFactory.Create();
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
            // Ignore session storage errors during initial restore.
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Username and password are required.");
        }

        var bruteKey = !string.IsNullOrWhiteSpace(ipAddress) ? $"ip:{ipAddress}" : $"user:{username.Trim().ToLowerInvariant()}";

        if (bruteForceService is not null && bruteForceService.IsBlocked(bruteKey, out var remainingTime))
        {
            var minutes = Math.Max(1, (int)Math.Ceiling(remainingTime?.TotalMinutes ?? 15));
            return (false, $"Too many failed login attempts. Please try again in {minutes} minutes.");
        }

        User? user;
        try
        {
            await using var repo = repositoryFacadeFactory.Create();
            user = await repo.Users.GetByLoginAsync(username).ConfigureAwait(false);
        }
        catch (Exception ex) when (
            ex is DbException ||
            ex is InvalidOperationException ||
            ex.GetType().Name.Contains("SocketException", StringComparison.OrdinalIgnoreCase) ||
            ex.GetType().Name.Contains("MySqlException", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Database connection is unavailable.");
        }

        if (user is null || !VerifyPassword(password, user.PasswordHash))
        {
            var attempts = bruteForceService?.RecordFailedAttempt(bruteKey) ?? 1;
            var isBlockedNow = bruteForceService?.IsBlocked(bruteKey, out _) ?? false;

            if (isBlockedNow)
            {
                var blockedPayload = new AuditChangePayload
                {
                    Summary = $"Brute-force alert! 5+ failed attempts for '{username}' from IP {ipAddress ?? "local"}. Account locked for 15 min.",
                    Severity = "Critical",
                    IpAddress = ipAddress,
                    Metadata = new() { ["attempts"] = attempts.ToString(System.Globalization.CultureInfo.InvariantCulture), ["username"] = username }
                };

                await auditService.LogActionSafeAsync("BRUTE_FORCE_BLOCKED", "User", user?.Id.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? username, blockedPayload.ToJson(), user?.Id).ConfigureAwait(false);
                return (false, "Too many failed login attempts. Please try again in 15 minutes.");
            }

            var failedPayload = new AuditChangePayload
            {
                Summary = $"Failed login attempt #{attempts} for user '{username}'. Invalid credentials.",
                Severity = "Warning",
                IpAddress = ipAddress,
                Metadata = new() { ["attempts"] = attempts.ToString(System.Globalization.CultureInfo.InvariantCulture), ["username"] = username }
            };

            await auditService.LogActionSafeAsync("LOGIN_FAILED", "User", user?.Id.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? username, failedPayload.ToJson(), user?.Id).ConfigureAwait(false);
            return (false, "Invalid username or password.");
        }

        if (!user.IsActive)
        {
            var deactivatedPayload = new AuditChangePayload
            {
                Summary = $"Deactivated account '{username}' attempted to log in.",
                Severity = "Warning",
                IpAddress = ipAddress
            };
            await auditService.LogActionSafeAsync("LOGIN_FAILED", "User", user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), deactivatedPayload.ToJson(), user.Id).ConfigureAwait(false);
            return (false, "Account is deactivated. Please contact administrator.");
        }

        bruteForceService?.Reset(bruteKey);

        this.CurrentUser = user;
        await this.SaveToSessionAsync(user).ConfigureAwait(false);
        this.AuthStateChanged?.Invoke(this, EventArgs.Empty);

        var loginPayload = new AuditChangePayload
        {
            Summary = $"User {user.Login} logged in successfully.",
            Severity = "Info",
            IpAddress = ipAddress
        };

        await auditService.LogActionSafeAsync("LOGIN", "User", user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), loginPayload.ToJson(), user.Id).ConfigureAwait(false);

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

        try
        {
            await using var repo = repositoryFacadeFactory.Create();
            var existingUser = await repo.Users.GetByLoginAsync(username).ConfigureAwait(false);

            if (existingUser is not null)
            {
                return (false, "Username already exists.");
            }

            ContactType? emailContactType = null;
            if (!string.IsNullOrWhiteSpace(email))
            {
                var contactTypes = await repo.ContactTypes.GetAllAsync().ConfigureAwait(false);
                emailContactType = contactTypes.FirstOrDefault(ct => ct.Name == "Email");

                if (emailContactType is not null)
                {
                    var existingEmail = await repo.ContactInfos.ExistsByEmailAsync(email, emailContactType.Id).ConfigureAwait(false);

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

            await repo.People.CreateAsync(person).ConfigureAwait(false);

            var user = new User
            {
                Login = username,
                PasswordHash = ComputeSecureHash(password),
                Role = UserRole.User,
                Person = person,
                IsActive = true,
            };

            await repo.Users.CreateAsync(user).ConfigureAwait(false);
            await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            await auditService.LogActionSafeAsync("REGISTER", "User", user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), $"User registered: {user.Login}", user.Id).ConfigureAwait(false);

            return (true, null);
        }
        catch (Exception ex) when (
            ex is DbException ||
            ex is InvalidOperationException ||
            ex.GetType().Name.Contains("SocketException", StringComparison.OrdinalIgnoreCase) ||
            ex.GetType().Name.Contains("MySqlException", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Database connection is unavailable.");
        }
    }

    public async Task LogoutAsync()
    {
        if (this.CurrentUser is not null)
        {
            await auditService.LogActionSafeAsync("LOGOUT", "User", this.CurrentUser.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), $"User {this.CurrentUser.Login} logged out", this.CurrentUser.Id).ConfigureAwait(false);
        }

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
            // Ignore session storage deletion errors.
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
            // Ignore session storage errors.
        }
    }
}
