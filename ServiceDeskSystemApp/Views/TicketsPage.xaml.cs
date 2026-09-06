using ServiceDeskSystemApp.ViewModels;

namespace ServiceDeskSystemApp.Views;

public partial class TicketsPage : ContentPage
{
    private readonly TicketsViewModel _viewModel;

    public TicketsPage(TicketsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadTicketsAsync();
    }

    private async void OnViewTicketClicked(object? sender, EventArgs e)
    {
        if (sender is View view && view.BindingContext is ServiceDeskSystemApp.Models.Tickets.TicketDto ticket)
        {
            await Shell.Current.GoToAsync($"{nameof(TicketDetailPage)}?id={ticket.Id}");
        }
    }
}
