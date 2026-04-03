namespace ServiceDeskSystem.Application.Services.Tickets.Models;

public sealed record DeveloperDashboardStatsDto(
    int AssignedCount,
    int InProgressCount,
    int CompletedCount);