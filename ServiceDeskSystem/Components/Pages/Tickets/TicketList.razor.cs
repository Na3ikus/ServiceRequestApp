using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Tickets;

/// <summary>
/// Ticket list page component.
/// </summary>
public partial class TicketList : BaseComponent
{
    private HubConnection? ticketsHubConnection;
    private bool ticketsHubInitialized;
    private bool isRefreshing;
    private string searchQueryValue = string.Empty;

    [Inject]
    protected ITicketService TicketService { get; set; } = null!;

    [Inject]
    protected IJSRuntime JS { get; set; } = null!;

    protected IList<Ticket>? Tickets { get; set; }

    protected IList<Ticket>? FilteredTickets { get; set; }

    protected string ViewMode { get; set; } = "Table";

    protected int CurrentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    protected bool IsAdmin => this.AuthService.CurrentUser?.Role == UserRole.Admin;

    protected string SearchQuery
    {
        get => this.searchQueryValue;
        set
        {
            if (this.searchQueryValue != value)
            {
                this.searchQueryValue = value;
                this.ApplyFilters();
            }
        }
    }

    protected string SelectedPriority { get; set; } = "All";

    protected string SelectedStatus { get; set; } = "All";

    protected string SelectedType { get; set; } = "All";

    protected override async Task OnInitializedAsync()
    {
        var currentUser = this.AuthService.CurrentUser;
        if (currentUser is null)
        {
            this.Navigation.NavigateTo("/login", replace: true);
            return;
        }

        if (currentUser.Role == UserRole.User)
        {
            this.Navigation.NavigateTo("/my-tickets", replace: true);
            return;
        }

        this.Tickets = await this.TicketService.GetAllTicketsAsync().ConfigureAwait(false);
        this.ApplyFilters();
        await this.StartTicketsHubAsync().ConfigureAwait(false);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                var defaultView = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.defaultView").ConfigureAwait(false);
                if (string.Equals(defaultView, "KANBAN", StringComparison.OrdinalIgnoreCase))
                {
                    this.ViewMode = "Kanban";
                    this.StateHasChanged();
                }
            }
            catch
            {
                // Ignore prerendering errors
            }
        }

        await base.OnAfterRenderAsync(firstRender).ConfigureAwait(false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _ = this.StopTicketsHubAsync();
        }

        base.Dispose(disposing);
    }

    protected async Task SetViewModeAsync(string mode)
    {
        this.ViewMode = mode;
        this.StateHasChanged();

        try
        {
            await this.JS.InvokeVoidAsync("localStorage.setItem", "settings.defaultView", mode.ToUpperInvariant()).ConfigureAwait(false);
        }
        catch
        {
            // Ignore prerendering errors
        }
    }

    protected async Task HandleKanbanStatusChangedAsync((int TicketId, TicketStatus NewStatus) args)
    {
        var ticket = this.Tickets?.FirstOrDefault(t => t.Id == args.TicketId);
        if (ticket is null)
        {
            return;
        }

        if (!this.IsAdmin && ticket.DeveloperId != this.CurrentUserId)
        {
            return;
        }

        var success = await this.TicketService.UpdateTicketStatusAsync(args.TicketId, args.NewStatus).ConfigureAwait(false);
        if (success)
        {
            ticket.Status = args.NewStatus;
            await this.InvokeAsync(this.StateHasChanged).ConfigureAwait(false);
        }
    }

    protected async Task StartTicketsHubAsync()
    {
        if (this.ticketsHubInitialized)
        {
            return;
        }

        this.ticketsHubConnection = new HubConnectionBuilder()
            .WithUrl(this.Navigation.ToAbsoluteUri("/hubs/updates"))
            .WithAutomaticReconnect()
            .Build();

        this.ticketsHubConnection.On("TicketsChanged", async () =>
        {
            await this.RefreshTicketsAsync().ConfigureAwait(false);
        });

        try
        {
            await this.ticketsHubConnection.StartAsync().ConfigureAwait(false);
            this.ticketsHubInitialized = true;
        }
        catch
        {
            this.ticketsHubConnection = null;
        }
    }

    protected async Task StopTicketsHubAsync()
    {
        if (this.ticketsHubConnection is null)
        {
            return;
        }

        try
        {
            await this.ticketsHubConnection.StopAsync().ConfigureAwait(false);
            await this.ticketsHubConnection.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
            // Ignore shutdown/reconnect races.
        }
        finally
        {
            this.ticketsHubConnection = null;
            this.ticketsHubInitialized = false;
        }
    }

    protected async Task RefreshTicketsAsync()
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
                this.Tickets = await this.TicketService.GetAllTicketsAsync().ConfigureAwait(false);
                this.ApplyFilters();
                this.StateHasChanged();
            }).ConfigureAwait(false);
        }
        finally
        {
            this.isRefreshing = false;
        }
    }

    protected void ApplyFilters()
    {
        if (this.Tickets is null)
        {
            this.FilteredTickets = null;
            return;
        }

        var query = this.Tickets.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(this.SearchQuery))
        {
            query = query.Where(t =>
                t.Title.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                t.Product?.Name.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) == true ||
                t.Author?.Login.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) == true);
        }

        if (this.SelectedPriority != "All" && Enum.TryParse<TicketPriority>(this.SelectedPriority, out var parsedPriority))
        {
            query = query.Where(t => t.Priority == parsedPriority);
        }

        if (this.SelectedStatus != "All")
        {
            if (this.SelectedStatus == "Open/InProgress")
            {
                query = query.Where(t => t.Status is TicketStatus.Open or TicketStatus.InProgress);
            }
            else if (this.SelectedStatus == "Closed/Resolved")
            {
                query = query.Where(t => t.Status is TicketStatus.Closed or TicketStatus.Resolved);
            }
            else if (Enum.TryParse<TicketStatus>(this.SelectedStatus.Replace(" ", string.Empty, StringComparison.Ordinal), out var parsedStatus))
            {
                query = query.Where(t => t.Status == parsedStatus);
            }
        }

        if (this.SelectedType != "All" && Enum.TryParse<TicketType>(this.SelectedType, out var parsedType))
        {
            query = query.Where(t => t.Type == parsedType);
        }

        this.FilteredTickets = query.ToList();
    }

    protected void OnPriorityChanged(ChangeEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        this.SelectedPriority = e.Value?.ToString() ?? "All";
        this.ApplyFilters();
        this.StateHasChanged();
    }

    protected void OnStatusChanged(ChangeEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        this.SelectedStatus = e.Value?.ToString() ?? "All";
        this.ApplyFilters();
        this.StateHasChanged();
    }

    protected void OnTypeChanged(ChangeEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        this.SelectedType = e.Value?.ToString() ?? "All";
        this.ApplyFilters();
        this.StateHasChanged();
    }

    protected void ClearFilters()
    {
        this.SearchQuery = string.Empty;
        this.SelectedPriority = "All";
        this.SelectedStatus = "All";
        this.SelectedType = "All";
        this.ApplyFilters();
        this.StateHasChanged();
    }
}
