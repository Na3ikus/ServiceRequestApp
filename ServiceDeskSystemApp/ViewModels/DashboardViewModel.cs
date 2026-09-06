using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using ServiceDeskSystemApp.Models.Tickets;
using ServiceDeskSystemApp.Services;

namespace ServiceDeskSystemApp.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ITicketService _ticketService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private int statsTotal;

    [ObservableProperty]
    private int statsOpen;

    [ObservableProperty]
    private int statsCritical;

    [ObservableProperty]
    private string userName = "User";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    public ObservableCollection<TicketDto> RecentTickets { get; } = new();

    public DashboardViewModel(ITicketService ticketService, IAuthService authService)
    {
        _ticketService = ticketService;
        _authService = authService;
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            UserName = await SecureStorage.Default.GetAsync("username") ?? "User";

            var stats = await _ticketService.GetStatsAsync();
            if (stats != null)
            {
                StatsTotal = stats.Total;
                StatsOpen = stats.Open;
                StatsCritical = stats.Critical;
            }

            var recent = await _ticketService.GetTicketsAsync(1, 5);
            RecentTickets.Clear();
            if (recent?.Items != null)
            {
                foreach (var ticket in recent.Items)
                {
                    RecentTickets.Add(ticket);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task GoToTicketAsync(int ticketId)
    {
        await Shell.Current.GoToAsync($"{nameof(Views.TicketDetailPage)}?id={ticketId}");
    }
}
