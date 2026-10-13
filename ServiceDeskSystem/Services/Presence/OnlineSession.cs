using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Services.Presence;

/// <summary>
/// One open browser tab (a Blazor circuit). <see cref="UserId"/> is <c>null</c> until someone signs in on it.
/// </summary>
#pragma warning disable SA1313 // Positional record parameters become PascalCase public properties.
public sealed record OnlineSession(
    string CircuitId,
    string? IpAddress,
    DateTime ConnectedAtUtc,
    int? UserId = null,
    string? Login = null,
    UserRole? Role = null,
    bool IsConnected = true);
#pragma warning restore SA1313
