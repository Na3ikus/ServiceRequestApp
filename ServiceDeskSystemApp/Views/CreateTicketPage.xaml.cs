using ServiceDeskSystemApp.ViewModels;

namespace ServiceDeskSystemApp.Views;

public partial class CreateTicketPage : ContentPage
{
    public CreateTicketPage(CreateTicketViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
