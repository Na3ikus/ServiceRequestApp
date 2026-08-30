using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceDeskSystemApp.Models.Tickets;
using ServiceDeskSystemApp.Services;

namespace ServiceDeskSystemApp.ViewModels;

public partial class TicketsViewModel : ObservableObject
{
    private readonly ITicketService _ticketService;

    public TicketsViewModel(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public ObservableCollection<TicketDto> Tickets { get; } = new();

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private int _statsTotal;

    [ObservableProperty]
    private int _statsOpen;

    [ObservableProperty]
    private int _statsCritical;

    [RelayCommand]
    public async Task LoadTicketsAsync()
    {
        IsRefreshing = true;

        var stats = await _ticketService.GetStatsAsync();
        if (stats != null)
        {
            StatsTotal = stats.Total;
            StatsOpen = stats.Open;
            StatsCritical = stats.Critical;
        }

        var pagedResult = await _ticketService.GetTicketsAsync(1, 10);
        if (pagedResult != null)
        {
            Tickets.Clear();
            foreach (var ticket in pagedResult.Items)
            {
                Tickets.Add(ticket);
            }
        }

        IsRefreshing = false;
    }
}
