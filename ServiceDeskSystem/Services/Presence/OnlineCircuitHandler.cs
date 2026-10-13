using System.Net;
using Microsoft.AspNetCore.Components.Server.Circuits;
using ServiceDeskSystem.Application.Services.Auth;

namespace ServiceDeskSystem.Services.Presence;

/// <summary>
/// Reports this tab's circuit to <see cref="OnlineSessionTracker"/>: opened with the client's IP, who is signed
/// in (following sign-in and sign-out), dropped and restored connections, and closed.
/// </summary>
public sealed class OnlineCircuitHandler(
    OnlineSessionTracker tracker,
    IAuthService authService,
    IHttpContextAccessor httpContextAccessor) : CircuitHandler, IDisposable
{
    /// <summary>
    /// Gets the id of this tab's circuit, so a page can tell its own row apart.
    /// </summary>
    public string? CircuitId { get; private set; }

    /// <summary>
    /// Formats the address for people: IPv4 clients reached over an IPv6 socket show as plain IPv4.
    /// </summary>
    public static string? FormatIp(IPAddress? address) =>
        address is null ? null : (address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address).ToString();

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(circuit);

        this.CircuitId = circuit.Id;

        // The circuit opens inside the request of its WebSocket, so this is the browser's address
        // (or, behind a proxy, the one UseForwardedHeaders took from X-Forwarded-For).
        tracker.Open(circuit.Id, FormatIp(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress));
        tracker.SetUser(circuit.Id, authService.CurrentUser);
        authService.AuthStateChanged += this.OnAuthStateChanged;
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(circuit);
        tracker.SetConnected(circuit.Id, false);
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(circuit);
        tracker.SetConnected(circuit.Id, true);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(circuit);
        this.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        authService.AuthStateChanged -= this.OnAuthStateChanged;

        if (this.CircuitId is not null)
        {
            tracker.Close(this.CircuitId);
        }
    }

    private void OnAuthStateChanged(object? sender, EventArgs e)
    {
        if (this.CircuitId is not null)
        {
            tracker.SetUser(this.CircuitId, authService.CurrentUser);
        }
    }
}
