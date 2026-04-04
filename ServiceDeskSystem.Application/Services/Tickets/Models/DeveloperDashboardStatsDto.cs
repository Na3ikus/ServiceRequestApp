namespace ServiceDeskSystem.Application.Services.Tickets.Models;

// Перенести в інше місце, API для отримання статистики по статусу тикетів для розробника
public sealed record DeveloperDashboardStatsDto(
    int AssignedCount,
    int InProgressCount,
    int CompletedCount);