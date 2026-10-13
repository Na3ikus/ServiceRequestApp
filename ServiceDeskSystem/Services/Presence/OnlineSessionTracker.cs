using System.Collections.Concurrent;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Services.Presence;

/// <summary>
/// The tabs open on this server right now, kept up to date by <see cref="OnlineCircuitHandler"/>.
/// <see cref="Changed"/> is raised on the thread that made the change.
/// </summary>
public sealed class OnlineSessionTracker(TimeProvider time)
{
    private readonly ConcurrentDictionary<string, OnlineSession> sessions = new ();

    public event EventHandler? Changed;

    /// <summary>
    /// Gets the open tabs: signed-in ones by login, then guests, each oldest connection first.
    /// </summary>
    public IReadOnlyList<OnlineSession> Sessions => this.sessions.Values
        .OrderBy(s => s.Login is null)
        .ThenBy(s => s.Login, StringComparer.OrdinalIgnoreCase)
        .ThenBy(s => s.ConnectedAtUtc)
        .ToList();

    public void Open(string circuitId, string? ipAddress)
    {
        this.sessions[circuitId] = new OnlineSession(circuitId, ipAddress, time.GetUtcNow().UtcDateTime);
        this.RaiseChanged();
    }

    public void SetUser(string circuitId, User? user) =>
        this.Update(circuitId, s => new OnlineSession(s.CircuitId, s.IpAddress, s.ConnectedAtUtc, user?.Id, user?.Login, user?.Role, s.IsConnected));

    /// <summary>
    /// Marks a tab whose connection dropped; Blazor keeps its circuit for a while in case it comes back.
    /// </summary>
    public void SetConnected(string circuitId, bool isConnected) =>
        this.Update(circuitId, s => new OnlineSession(s.CircuitId, s.IpAddress, s.ConnectedAtUtc, s.UserId, s.Login, s.Role, isConnected));

    public void Close(string circuitId)
    {
        if (this.sessions.TryRemove(circuitId, out _))
        {
            this.RaiseChanged();
        }
    }

    private void Update(string circuitId, Func<OnlineSession, OnlineSession> change)
    {
        while (this.sessions.TryGetValue(circuitId, out var current))
        {
            var updated = change(current);
            if (updated == current)
            {
                return;
            }

            if (this.sessions.TryUpdate(circuitId, updated, current))
            {
                this.RaiseChanged();
                return;
            }
        }
    }

    // A listener is an admin page; one that throws (its circuit is closing) must not break the others or
    // the circuit event that caused the change.
    private void RaiseChanged()
    {
        foreach (var handler in this.Changed?.GetInvocationList() ?? [])
        {
            try
            {
                ((EventHandler)handler)(this, EventArgs.Empty);
            }
            catch
            {
                // See above.
            }
        }
    }
}
