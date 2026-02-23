namespace ServiceDeskSystem.Application.Services.Tickets.Interfaces;

/// <summary>
/// Provides read-only analytics and aggregate counts for tickets.
/// Isolated so this layer can be replaced with dedicated caching or DB views independently.
/// </summary>
public interface ITicketStatisticsService
{
    Task<int> GetTotalTicketsCountAsync();

    Task<int> GetOpenTicketsCountAsync();

    Task<int> GetCriticalTicketsCountAsync();

    Task<int> GetUserTicketsCountAsync(int userId);

    Task<int> GetDeveloperAssignedCountAsync(int developerId);

    Task<int> GetDeveloperInProgressCountAsync(int developerId);

    Task<int> GetDeveloperCompletedCountAsync(int developerId);

    Task<Dictionary<string, int>> GetTicketCountByStatusAsync();

    Task<Dictionary<string, int>> GetTicketCountByPriorityAsync();

    Task<List<(string Login, int Count)>> GetTopDevelopersAsync(int top = 5);
}
