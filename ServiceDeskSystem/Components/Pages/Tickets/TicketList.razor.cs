using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Components.Features.Tickets;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Components.UI.Primitives;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Tickets;

/// <summary>
/// Ticket list page component.
/// </summary>
public partial class TicketList : BaseComponent
{
    protected const string TableView = "TABLE";

    protected const string KanbanView = "KANBAN";

    private static readonly IReadOnlyList<PathSegment> Segments = [new ("tickets")];

    private HubConnection? ticketsHubConnection;
    private bool ticketsHubInitialized;
    private bool isRefreshing;

    [Inject]
    protected ITicketService TicketService { get; set; } = null!;

    [Inject]
    protected IJSRuntime JS { get; set; } = null!;

    protected IList<Ticket>? Tickets { get; set; }

    protected IReadOnlyList<Ticket> FilteredTickets { get; set; } = [];

    protected TicketFilterState Filter { get; set; } = new ();

    protected string ViewMode { get; set; } = TableView;

    protected IReadOnlyList<(string Value, string Text)> ViewOptions =>
        [(TableView, this.Heading("kanban.viewTable")), (KanbanView, this.Heading("kanban.viewKanban"))];

    protected int CurrentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    protected bool IsAdmin => this.AuthService.CurrentUser?.Role == UserRole.Admin;

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

        this.Tickets = await this.TicketService.GetAllTicketsAsync();
        this.ApplyFilters();
        await this.StartTicketsHubAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                var defaultView = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.defaultView");
                if (string.Equals(defaultView, KanbanView, StringComparison.OrdinalIgnoreCase))
                {
                    this.ViewMode = KanbanView;
                    this.StateHasChanged();
                }
            }
            catch
            {
                // Ignore prerendering errors
            }
        }

        await base.OnAfterRenderAsync(firstRender);
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
            await this.JS.InvokeVoidAsync("localStorage.setItem", "settings.defaultView", mode);
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

        var success = await this.TicketService.UpdateTicketStatusAsync(args.TicketId, args.NewStatus, this.AuthService.CurrentUser?.Id);
        if (success)
        {
            ticket.Status = args.NewStatus;
            this.ApplyFilters();
            await this.InvokeAsync(this.StateHasChanged);
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
            await this.RefreshTicketsAsync();
        });

        try
        {
            await this.ticketsHubConnection.StartAsync();
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
            await this.ticketsHubConnection.StopAsync();
            await this.ticketsHubConnection.DisposeAsync();
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
                this.Tickets = await this.TicketService.GetAllTicketsAsync();
                this.ApplyFilters();
                this.StateHasChanged();
            });
        }
        finally
        {
            this.isRefreshing = false;
        }
    }

    protected void ApplyFilters() =>
        this.FilteredTickets = this.Tickets is null ? [] : TicketFilter.Apply(this.Tickets, this.Filter);

    protected void SetFilter(TicketFilterState filter)
    {
        this.Filter = filter;
        this.ApplyFilters();
    }
}
