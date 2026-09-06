using System;
using System.Collections.Concurrent;

namespace ServiceDeskSystem.Application.Services.Auth;

/// <summary>
/// Thread-safe in-memory brute force protection tracking failed login attempts.
/// </summary>
public sealed class BruteForceProtectionService : IBruteForceProtectionService
{
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(15);

    private readonly ConcurrentDictionary<string, AttemptRecord> attempts = new(StringComparer.OrdinalIgnoreCase);

    public bool IsBlocked(string key, out TimeSpan? remainingTime)
    {
        remainingTime = null;

        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (this.attempts.TryGetValue(key, out var record))
        {
            var now = DateTime.UtcNow;

            if (record.LockedUntil.HasValue)
            {
                if (record.LockedUntil.Value > now)
                {
                    remainingTime = record.LockedUntil.Value - now;
                    return true;
                }

                // Lockout expired, clear entry
                this.attempts.TryRemove(key, out _);
                return false;
            }
        }

        return false;
    }

    public int RecordFailedAttempt(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }

        var now = DateTime.UtcNow;

        var updated = this.attempts.AddOrUpdate(
            key,
            _ => new AttemptRecord(1, now, null),
            (_, existing) =>
            {
                // If previous attempts occurred outside the attempt window, reset counter
                var count = (now - existing.LastAttemptAt <= AttemptWindow) ? existing.Count + 1 : 1;
                DateTime? lockedUntil = null;

                if (count >= MaxFailedAttempts)
                {
                    lockedUntil = now.Add(LockoutDuration);
                }

                return new AttemptRecord(count, now, lockedUntil);
            });

        return updated.Count;
    }

    public void Reset(string key)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            this.attempts.TryRemove(key, out _);
        }
    }

    public int GetFailedAttemptCount(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }

        if (this.attempts.TryGetValue(key, out var record))
        {
            if (DateTime.UtcNow - record.LastAttemptAt <= AttemptWindow)
            {
                return record.Count;
            }
        }

        return 0;
    }

    private sealed record AttemptRecord(int Count, DateTime LastAttemptAt, DateTime? LockedUntil);
}
