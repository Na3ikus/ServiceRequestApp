using ServiceDeskSystemApp.Views;

namespace ServiceDeskSystemApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(TicketDetailPage), typeof(TicketDetailPage));
        Routing.RegisterRoute(nameof(CreateTicketPage), typeof(CreateTicketPage));
        Routing.RegisterRoute(nameof(TicketsPage), typeof(TicketsPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
    }
}
