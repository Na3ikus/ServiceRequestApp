using System.Threading;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Components.Common;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Tickets;

namespace ServiceDeskSystem.Components.Pages.Developer;

/// <summary>
/// Developer dashboard page component.
/// </summary>
public partial class DeveloperDashboard : BaseComponent
{
    private readonly TimeSpan refreshInterval = TimeSpan.FromSeconds(5);
    private Timer? refreshTimer;
    private bool isRefreshing;
    private bool authRestored;

    [Inject]
    private ITicketService TicketService { get; set; } = null!;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private List<Ticket>? tickets { get; set; }

    private int assignedCount { get; set; }

    private int inProgressCount { get; set; }

    private int completedCount { get; set; }

    private int CurrentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    private string CurrentUserRole => this.AuthService.CurrentUser?.Role ?? string.Empty;

    private bool IsDeveloper => string.Equals(this.CurrentUserRole, "Developer", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(this.CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase);
    protected override async Task OnInitializedAsync()
    {
        this.AuthService.AuthStateChanged += this.OnAuthStateChanged;

        await this.LoadDataAsync();
        this.StartAutoRefresh();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !this.authRestored)
        {
            await this.AuthService.EnsureRestoredAsync();
            this.authRestored = true;
            await this.LoadDataAsync();
            await this.InvokeAsync(this.StateHasChanged);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.AuthService.AuthStateChanged -= this.OnAuthStateChanged;
            this.refreshTimer?.Dispose();
        }

        base.Dispose(disposing);
    }


    private async Task LoadDataAsync()
    {
        if (!this.IsDeveloper || this.CurrentUserId == 0)
        {
            return;
        }

        this.tickets = await this.TicketService.GetDeveloperTicketsAsync(this.CurrentUserId);
        this.assignedCount = await this.TicketService.GetDeveloperAssignedCountAsync(this.CurrentUserId);
        this.inProgressCount = await this.TicketService.GetDeveloperInProgressCountAsync(this.CurrentUserId);
        this.completedCount = await this.TicketService.GetDeveloperCompletedCountAsync(this.CurrentUserId);
    }

    private void StartAutoRefresh()
    {
        this.refreshTimer ??= new Timer(async _ => await this.RefreshAsync(), null, this.refreshInterval, this.refreshInterval);
    }

    private async Task RefreshAsync()
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
                await this.LoadDataAsync();
                this.StateHasChanged();
            });
        }
        finally
        {
            this.isRefreshing = false;
        }
    }

    private void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);

    private async void OnAuthStateChanged(object? sender, EventArgs e)
    {
        await this.LoadDataAsync();
        await this.InvokeAsync(this.StateHasChanged);
    }

    private void ViewTicket(int id) => this.Navigation.NavigateTo($"/ticket/{id}");


}

