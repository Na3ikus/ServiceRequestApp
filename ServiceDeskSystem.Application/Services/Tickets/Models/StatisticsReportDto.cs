using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Application.Services.Tickets.Models;

/// <summary>
/// Everything the statistics page shows for one period: the last <see cref="Days"/> days in the user's zone,
/// <see cref="From"/> to <see cref="To"/> (today) inclusive.
/// </summary>
public sealed record StatisticsReportDto(
    int Days,
    DateOnly From,
    DateOnly To,
    StatisticsSnapshotDto Snapshot,
    StatisticsPeriodDto Period,
    IReadOnlyList<DailyFlowDto> Trend,
    IReadOnlyList<ProductStatisticsDto> Products,
    IReadOnlyList<DeveloperStatisticsDto> Team);

/// <summary>
/// Current state, independent of the period. Priority, type and tag counts cover open tickets only;
/// the status, priority and type maps contain every enum value.
/// </summary>
public sealed record StatisticsSnapshotDto(
    int Open,
    int CriticalOpen,
    IReadOnlyDictionary<TicketStatus, int> ByStatus,
    IReadOnlyDictionary<TicketPriority, int> ByPriority,
    IReadOnlyDictionary<TicketType, int> ByType,
    IReadOnlyList<TagCountDto> Tags);

/// <summary>
/// One tag and the number of open tickets carrying it.
/// </summary>
public sealed record TagCountDto(int TagId, string Name, int Count);

/// <summary>
/// Period figures. <see cref="AverageResolution"/> is <c>null</c> when nothing was resolved,
/// <see cref="ResolvedPercent"/> when nothing was created.
/// </summary>
public sealed record StatisticsPeriodDto(
    int Created,
    int Resolved,
    TimeSpan? AverageResolution,
    int? ResolvedPercent);

/// <summary>
/// Tickets created and resolved on one day in the user's zone.
/// </summary>
public sealed record DailyFlowDto(DateOnly Date, int Created, int Resolved);

/// <summary>
/// One product: open now, created and resolved in the period, mean resolution time of those resolved.
/// </summary>
public sealed record ProductStatisticsDto(
    int ProductId,
    string Name,
    int Open,
    int Created,
    int Resolved,
    TimeSpan? AverageResolution);

/// <summary>
/// One developer (or admin) row of the team table.
/// </summary>
public sealed record DeveloperStatisticsDto(
    int UserId,
    string Login,
    string DisplayName,
    int InProgress,
    int Active,
    int ClosedInPeriod,
    int MinutesInPeriod)
{
    /// <summary>
    /// Gets the workload score: in progress × 2 + other active.
    /// </summary>
    public int Workload => (this.InProgress * 2) + (this.Active - this.InProgress);

    /// <summary>
    /// Gets the logged minutes per ticket closed in the period; <c>null</c> when none was closed.
    /// </summary>
    public double? AverageMinutesPerTicket => this.ClosedInPeriod > 0 ? (double)this.MinutesInPeriod / this.ClosedInPeriod : null;

    /// <summary>
    /// Gets a value indicating whether every figure of the row is zero.
    /// </summary>
    public bool IsIdle => this.InProgress == 0 && this.Active == 0 && this.ClosedInPeriod == 0 && this.MinutesInPeriod == 0;
}
