using ServiceDeskSystemApp.Views;

namespace ServiceDeskSystemApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute(nameof(TicketDetailPage), typeof(TicketDetailPage));
    }
}
