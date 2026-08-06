using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Application.Services.Tickets.Models;

public sealed record ExtendedAnalyticsDto(
    AnalyticsKpiDto Kpis,
    List<DailyTicketTrendDto> Trends,
    List<DeveloperWorkloadDto> DeveloperWorkloads,
    List<ProductResolutionPerformanceDto> ProductPerformances,
    List<TagAnalyticsDto> TagDistributions,
    List<TicketTypeAnalyticsDto> TypeDistributions
);

public sealed record TicketTypeAnalyticsDto(
    TicketType Type,
    string TypeName,
    int TicketCount,
    double Percentage
);

public sealed record AnalyticsKpiDto(
    int TotalTickets,
    int OpenTickets,
    int ResolvedTickets,
    double AvgResolutionHours,
    int ActiveDevelopersCount,
    double ResolutionRatePercent
);

public sealed record DailyTicketTrendDto(
    DateTime Date,
    string DateLabel,
    int CreatedCount,
    int ResolvedCount
);

public sealed record DeveloperWorkloadDto(
    int DeveloperId,
    string Login,
    int AssignedCount,
    int InProgressCount,
    int ResolvedCount,
    double WorkloadScore
);

public sealed record ProductResolutionPerformanceDto(
    int ProductId,
    string ProductName,
    int TotalTickets,
    int ResolvedTickets,
    double AvgResolutionHours
);

public sealed record TagAnalyticsDto(
    int TagId,
    string TagName,
    string Color,
    int TicketCount
);
