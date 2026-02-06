using System.Threading;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Components.Common;
using ServiceDeskSystem.Data.Entities;
using ServiceDeskSystem.Services.Localization;
using ServiceDeskSystem.Services.Tickets;

namespace ServiceDeskSystem.Components.Pages.Tickets;

/// <summary>
/// Ticket list page component.
/// </summary>
public partial class TicketList : BaseComponent
{
    private readonly TimeSpan refreshInterval = TimeSpan.FromSeconds(5);
    private Timer? refreshTimer;
    private bool isRefreshing;

    [Inject]
    private ITicketService TicketService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private List<Ticket>? tickets { get; set; }

    private List<Ticket>? filteredTickets { get; set; }

    private string searchQuery { get; set; } = string.Empty;

    private string selectedPriority { get; set; } = "All";

    private string selectedStatus { get; set; } = "All";

    protected override async Task OnInitializedAsync()
    {
        this.tickets = await this.TicketService.GetAllTicketsAsync();
        this.ApplyFilters();
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

    private static string GetStatusBadgeClass(string status) => status switch
    {
        "Open" => "bg-blue-100 text-blue-800 dark:bg-blue-900/50 dark:text-blue-300",
        "In Progress" => "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/50 dark:text-yellow-300",
        "Resolved" => "bg-green-100 text-green-800 dark:bg-green-900/50 dark:text-green-300",
        "Closed" => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
        _ => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
    };

    private static string GetPriorityBadgeClass(string priority) => priority switch
    {
        "Critical" => "bg-red-100 text-red-800 dark:bg-red-900/50 dark:text-red-300",
        "High" => "bg-orange-100 text-orange-800 dark:bg-orange-900/50 dark:text-orange-300",
        "Medium" => "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/50 dark:text-yellow-300",
        "Low" => "bg-green-100 text-green-800 dark:bg-green-900/50 dark:text-green-300",
        _ => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
    };

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
                this.tickets = await this.TicketService.GetAllTicketsAsync();
                this.ApplyFilters();
                this.StateHasChanged();
            });
        }
        finally
        {
            this.isRefreshing = false;
        }
    }

    private void ApplyFilters()
    {
        if (this.tickets is null)
        {
            this.filteredTickets = null;
            return;
        }

        var query = this.tickets.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(this.searchQuery))
        {
            var search = this.searchQuery.ToLowerInvariant();
            query = query.Where(t =>
                t.Title.ToLowerInvariant().Contains(search) ||
                t.Description.ToLowerInvariant().Contains(search) ||
                t.Product?.Name.ToLowerInvariant().Contains(search) == true ||
                t.Author?.Login.ToLowerInvariant().Contains(search) == true);
        }

        if (this.selectedPriority != "All")
        {
            query = query.Where(t => t.Priority == this.selectedPriority);
        }

        if (this.selectedStatus != "All")
        {
            if (this.selectedStatus == "Open/InProgress")
            {
                query = query.Where(t => t.Status == "Open" || t.Status == "In Progress");
            }
            else if (this.selectedStatus == "Closed/Resolved")
            {
                query = query.Where(t => t.Status == "Closed" || t.Status == "Resolved");
            }
            else
            {
                query = query.Where(t => t.Status == this.selectedStatus);
            }
        }

        this.filteredTickets = query.ToList();
    }

    private void OnSearchChanged(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        this.searchQuery = e.Value?.ToString() ?? string.Empty;
        this.ApplyFilters();
    }

    private void OnPriorityChanged(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        this.selectedPriority = e.Value?.ToString() ?? "All";
        this.ApplyFilters();
    }

    private void OnStatusChanged(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        this.selectedStatus = e.Value?.ToString() ?? "All";
        this.ApplyFilters();
    }

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
