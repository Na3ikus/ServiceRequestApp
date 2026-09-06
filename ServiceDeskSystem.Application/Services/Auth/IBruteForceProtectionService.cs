using System;

namespace ServiceDeskSystem.Application.Services.Auth;

/// <summary>
/// Service to protect against brute force login attacks.
/// </summary>
public interface IBruteForceProtectionService
{
    /// <summary>
    /// Checks if the given key (e.g. IP or username) is currently locked out.
    /// </summary>
    bool IsBlocked(string key, out TimeSpan? remainingTime);

    /// <summary>
    /// Records a failed login attempt and returns the current attempt count.
    /// </summary>
    int RecordFailedAttempt(string key);

    /// <summary>
    /// Resets failed login attempts for the given key upon successful login.
    /// </summary>
    void Reset(string key);

    /// <summary>
    /// Gets the current failed attempt count.
    /// </summary>
    int GetFailedAttemptCount(string key);
}
