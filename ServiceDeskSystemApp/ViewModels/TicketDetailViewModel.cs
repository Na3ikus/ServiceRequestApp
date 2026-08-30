using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceDeskSystemApp.Models.Tickets;
using ServiceDeskSystemApp.Services;

namespace ServiceDeskSystemApp.ViewModels;

[QueryProperty(nameof(TicketId), "id")]
public partial class TicketDetailViewModel : ObservableObject
{
    private readonly ITicketService _ticketService;

    public TicketDetailViewModel(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [ObservableProperty]
    private int _ticketId;

    [ObservableProperty]
    private TicketDto? _ticket;

    [ObservableProperty]
    private bool _isLoading;

    partial void OnTicketIdChanged(int value)
    {
        _ = LoadTicketAsync();
    }

    [RelayCommand]
    private async Task LoadTicketAsync()
    {
        IsLoading = true;
        Ticket = await _ticketService.GetTicketByIdAsync(TicketId);
        IsLoading = false;
    }
}
