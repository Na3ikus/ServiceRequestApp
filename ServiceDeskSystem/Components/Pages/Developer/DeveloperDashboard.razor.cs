using System.Threading;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Components.Common;
using ServiceDeskSystem.Data.Entities;
using ServiceDeskSystem.Services.Auth;
using ServiceDeskSystem.Services.Tickets;

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

    private bool IsDeveloper => string.Equals(this.CurrentUserRole, "Developer", StringComparison.OrdinalIgnoreCase);
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

    private static string GetStatusBadgeClass(string status) => status switch
    {
        "New" => "bg-purple-200 text-purple-900 dark:bg-purple-900 dark:text-purple-200 font-semibold",
        "Open" => "bg-blue-200 text-blue-900 dark:bg-blue-900 dark:text-blue-200 font-semibold",
        "In Progress" => "bg-yellow-200 text-yellow-900 dark:bg-yellow-900 dark:text-yellow-200 font-semibold",
        "Resolved" => "bg-green-200 text-green-900 dark:bg-green-900 dark:text-green-200 font-semibold",
        "Closed" => "bg-gray-200 text-gray-900 dark:bg-gray-700 dark:text-gray-200 font-semibold",
        "Testing" => "bg-cyan-200 text-cyan-900 dark:bg-cyan-900 dark:text-cyan-200 font-semibold",
        "Code Review" => "bg-indigo-200 text-indigo-900 dark:bg-indigo-900 dark:text-indigo-200 font-semibold",
        "Done" => "bg-emerald-200 text-emerald-900 dark:bg-emerald-900 dark:text-emerald-200 font-semibold",
        _ => "bg-gray-200 text-gray-900 dark:bg-gray-700 dark:text-gray-200 font-semibold",
    };

    private static string GetPriorityBadgeClass(string priority) => priority switch
    {
        "Critical" => "bg-red-100 text-red-800 dark:bg-red-900/50 dark:text-red-300",
        "High" => "bg-orange-100 text-orange-800 dark:bg-orange-900/50 dark:text-orange-300",
        "Medium" => "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/50 dark:text-yellow-300",
        "Low" => "bg-green-100 text-green-800 dark:bg-green-900/50 dark:text-green-300",
        _ => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
    };

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

    private string GetStatusText(string status) => status switch
    {
        "Open" => this.L.Translate("status.open"),
        "In Progress" => this.L.Translate("status.inProgress"),
        "Resolved" => this.L.Translate("status.resolved"),
        "Closed" => this.L.Translate("status.closed"),
        _ => status,
    };

    private string GetPriorityText(string priority) => priority switch
    {
        "Low" => this.L.Translate("priority.low"),
        "Medium" => this.L.Translate("priority.medium"),
        "High" => this.L.Translate("priority.high"),
        "Critical" => this.L.Translate("priority.critical"),
        _ => priority,
    };
}
