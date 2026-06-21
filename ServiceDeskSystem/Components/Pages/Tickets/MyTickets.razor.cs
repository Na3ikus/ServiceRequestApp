using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Pages.Tickets;

/// <summary>
/// My tickets page component for displaying user's own tickets.
/// </summary>
public partial class MyTickets : BaseComponent
{
    private readonly TimeSpan refreshInterval = TimeSpan.FromSeconds(5);
    private Timer? refreshTimer;
    private bool isRefreshing;

    [Inject]
    private ITicketService TicketService { get; set; } = null!;

    private List<Ticket>? tickets { get; set; }

    private int CurrentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    protected override async Task OnInitializedAsync()
    {
        var currentUser = this.AuthService.CurrentUser;
        if (currentUser is null)
        {
            this.Navigation.NavigateTo("/login", replace: true);
            return;
        }

        if (currentUser.Role != Domain.Enums.UserRole.User)
        {
            // Admin and Developer have the full ticket list.
            this.Navigation.NavigateTo("/tickets", replace: true);
            return;
        }

        this.tickets = await this.TicketService.GetUserTicketsAsync(currentUser.Id);
        this.StartAutoRefresh();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.refreshTimer?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void StartAutoRefresh()
    {
        this.refreshTimer ??= new Timer(async _ => await this.RefreshTicketsAsync(), null, this.refreshInterval, this.refreshInterval);
    }

    private async Task RefreshTicketsAsync()
    {
        if (this.isRefreshing)
        {
            return;
        }

        this.isRefreshing = true;
        try
        {
            await this.InvokeAsync(async () =>
            {
                if (this.CurrentUserId != 0)
                {
                    this.tickets = await this.TicketService.GetUserTicketsAsync(this.CurrentUserId);
                    this.StateHasChanged();
                }
            });
        }
        finally
        {
            this.isRefreshing = false;
        }
    }
}
