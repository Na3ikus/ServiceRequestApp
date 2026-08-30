using ServiceDeskSystemApp.Models.Common;
using ServiceDeskSystemApp.Models.Tickets;

namespace ServiceDeskSystemApp.Services;

public class TicketService : ITicketService
{
    private readonly ApiService _apiService;

    public TicketService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<PagedResult<TicketDto>?> GetTicketsAsync(int page, int pageSize) 
        => await _apiService.GetAsync<PagedResult<TicketDto>>($"/api/tickets?page={page}&pageSize={pageSize}");

    public async Task<TicketDto?> GetTicketByIdAsync(int id) 
        => await _apiService.GetAsync<TicketDto>($"/api/tickets/{id}");

    public async Task<TicketDto?> CreateTicketAsync(CreateTicketRequest request) 
        => await _apiService.PostAsync<CreateTicketRequest, TicketDto>("/api/tickets", request);

    public async Task<TicketStatsDto?> GetStatsAsync() 
        => await _apiService.GetAsync<TicketStatsDto>("/api/tickets/stats");
}
