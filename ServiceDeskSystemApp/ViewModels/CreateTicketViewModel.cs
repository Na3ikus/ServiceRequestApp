using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceDeskSystemApp.Models.Tickets;
using ServiceDeskSystemApp.Services;
using ServiceDeskSystemApp.Models;

namespace ServiceDeskSystemApp.ViewModels;

public partial class CreateTicketViewModel : ObservableObject
{
    private readonly ITicketService _ticketService;

    public CreateTicketViewModel(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private TicketPriority _priority = TicketPriority.Medium;

    [ObservableProperty]
    private TicketType _type = TicketType.Support;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task SaveTicketAsync()
    {
        if (string.IsNullOrWhiteSpace(Title)) return;

        IsBusy = true;
        
        var request = new CreateTicketRequest
        {
            Title = Title,
            Description = Description,
            Priority = Priority,
            TicketType = Type,
            ProductId = 1 // default for demo
        };

        var result = await _ticketService.CreateTicketAsync(request);
        IsBusy = false;

        if (result != null)
        {
            await Shell.Current.GoToAsync(".."); // go back
        }
    }
}
