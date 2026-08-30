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

    private async void OnViewTicketClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int ticketId)
        {
            await Shell.Current.GoToAsync($"{nameof(TicketDetailPage)}?id={ticketId}");
        }
    }

    private async void OnCreateTicketClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CreateTicketPage));
    }
}
