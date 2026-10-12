using System;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.UI.Primitives;

/// <summary>
/// Maps ticket domain enums to the <see cref="Tone"/> used to render them as a <see cref="Tag"/>.
/// </summary>
public static class TicketTones
{
    public static Tone For(TicketStatus status) => status switch
    {
        TicketStatus.New => Tone.Accent,
        TicketStatus.Open => Tone.Active,
        TicketStatus.InProgress => Tone.Active,
        TicketStatus.Testing => Tone.Active,
        TicketStatus.CodeReview => Tone.Active,
        TicketStatus.Resolved => Tone.Muted,
        TicketStatus.Done => Tone.Muted,
        TicketStatus.Closed => Tone.Muted,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };

    public static Tone For(TicketPriority priority) => priority switch
    {
        TicketPriority.Critical => Tone.Danger,
        TicketPriority.High => Tone.Active,
        TicketPriority.Medium => Tone.Muted,
        TicketPriority.Low => Tone.Muted,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
    };

    public static Tone For(TicketType type) => type switch
    {
        TicketType.Bug => Tone.Active,
        TicketType.Support => Tone.Muted,
        TicketType.Project => Tone.Muted,
        TicketType.Consultation => Tone.Muted,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };
}
