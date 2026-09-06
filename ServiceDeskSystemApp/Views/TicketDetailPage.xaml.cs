using ServiceDeskSystemApp.ViewModels;

namespace ServiceDeskSystemApp.Views;

public partial class TicketDetailPage : ContentPage
{
    private readonly TicketDetailViewModel _viewModel;

    public TicketDetailPage(TicketDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}
