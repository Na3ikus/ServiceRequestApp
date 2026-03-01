using System.Threading;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Localization;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Tickets.Interfaces;
using ServiceDeskSystem.Components.Features;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Pages.Tickets;

/// <summary>
/// Ticket list page component.
/// </summary>
public partial class TicketList : BaseComponent
{
    private readonly TimeSpan refreshInterval = TimeSpan.FromSeconds(5);
    private Timer? refreshTimer;
    private bool isRefreshing;
    private string _searchQuery = string.Empty;

    [Inject]
    private ITicketService TicketService { get; set; } = null!;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private List<Ticket>? tickets { get; set; }

    private List<Ticket>? filteredTickets { get; set; }

    private string viewMode { get; set; } = "Table";

    private int currentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    private bool isAdmin => string.Equals(this.AuthService.CurrentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase);

    private string searchQuery
    {
        get => this._searchQuery;
        set
        {
            if (this._searchQuery != value)
            {
                this._searchQuery = value;
                this.ApplyFilters();
            }
        }
    }

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

    private void ViewTicket(int id) => this.Navigation.NavigateTo($"/ticket/{id}");

    private void SetViewMode(string mode)
    {
        this.viewMode = mode;
        this.StateHasChanged();
    }

    private async Task HandleKanbanStatusChangedAsync((int TicketId, string NewStatus) args)
    {
        var ticket = this.tickets?.FirstOrDefault(t => t.Id == args.TicketId);
        if (ticket is null)
        {
            return;
        }

        if (!this.isAdmin && ticket.DeveloperId != this.currentUserId)
        {
            return;
        }

        var success = await this.TicketService.UpdateTicketStatusAsync(args.TicketId, args.NewStatus);
        if (success)
        {
            ticket.Status = args.NewStatus;
            await this.InvokeAsync(this.StateHasChanged);
        }
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
            query = query.Where(t =>
                t.Title.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase) ||
                t.Product?.Name.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase) == true ||
                t.Author?.Login.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase) == true);
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

    private void OnPriorityChanged(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        this.selectedPriority = e.Value?.ToString() ?? "All";
        this.ApplyFilters();
        this.StateHasChanged();
    }

    private void OnStatusChanged(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        this.selectedStatus = e.Value?.ToString() ?? "All";
        this.ApplyFilters();
        this.StateHasChanged();
    }

    private void ClearFilters()
    {
        this.searchQuery = string.Empty;
        this.selectedPriority = "All";
        this.selectedStatus = "All";
        this.ApplyFilters();
        this.StateHasChanged();
    }
}
