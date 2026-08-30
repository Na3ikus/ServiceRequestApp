using ServiceDeskSystemApp.ViewModels;

namespace ServiceDeskSystemApp.Views;

public partial class TicketDetailPage : ContentPage
{
    public TicketDetailPage(TicketDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
