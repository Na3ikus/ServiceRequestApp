using System.Threading;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Localization;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Components.Common;
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

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private List<Ticket>? tickets { get; set; }

    private int CurrentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    protected override async Task OnInitializedAsync()
    {
        if (this.CurrentUserId == 0)
        {
            this.Navigation.NavigateTo("/login");
            return;
        }

        this.tickets = await this.TicketService.GetUserTicketsAsync(this.CurrentUserId);
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

    private void ViewTicket(int id) => this.Navigation.NavigateTo($"/ticket/{id}");

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
