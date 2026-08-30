using ServiceDeskSystemApp.Models.Common;
using ServiceDeskSystemApp.Models.Tickets;

namespace ServiceDeskSystemApp.Services;

public interface ITicketService
{
    Task<PagedResult<TicketDto>?> GetTicketsAsync(int page, int pageSize);
    Task<TicketDto?> GetTicketByIdAsync(int id);
    Task<TicketDto?> CreateTicketAsync(CreateTicketRequest request);
    Task<TicketStatsDto?> GetStatsAsync();
}
