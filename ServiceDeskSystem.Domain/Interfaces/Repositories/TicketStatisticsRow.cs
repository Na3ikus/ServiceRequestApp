using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Domain.Interfaces;

/// <summary>
/// The fields of a ticket the statistics report needs, projected in the database (no text columns).
/// </summary>
public sealed record TicketStatisticsRow(
    int Id,
    DateTime CreatedAt,
    TicketStatus Status,
    TicketPriority Priority,
    TicketType Type,
    int? DeveloperId,
    int? ProductId,
    string? ProductName,
    IReadOnlyList<TicketTagRow> Tags);

/// <summary>
/// A tag on a <see cref="TicketStatisticsRow"/>.
/// </summary>
public sealed record TicketTagRow(int Id, string Name);
