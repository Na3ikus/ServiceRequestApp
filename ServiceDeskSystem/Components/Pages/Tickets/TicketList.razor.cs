using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Realtime;
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

    private bool liveSubscribed;
    private bool isRefreshing;
    private bool refreshPending;

    [Inject]
    protected ITicketService TicketService { get; set; } = null!;

    [Inject]
    protected IJSRuntime JS { get; set; } = null!;

    [Inject]
    protected IRealtimeEvents RealtimeEvents { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

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
        this.StartLiveUpdates();
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
            this.StopLiveUpdates();
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

        var result = await this.TicketService.UpdateTicketStatusAsync(args.TicketId, args.NewStatus, this.AuthService.CurrentUser?.Id, ticket.Version);
        if (result == TicketUpdateResult.Updated)
        {
            ticket.Status = args.NewStatus;
            ticket.Version++;
            this.ApplyFilters();
            await this.InvokeAsync(this.StateHasChanged);
        }
        else if (result == TicketUpdateResult.Conflict)
        {
            await this.RefreshTicketsAsync();
            await this.ToastService.ShowToastAsync(this.L.Translate("details.conflict"), ToastType.Warning);
        }
    }

    protected void StartLiveUpdates()
    {
        if (this.liveSubscribed)
        {
            return;
        }

        this.RealtimeEvents.TicketsChanged += this.OnTicketsChanged;
        this.liveSubscribed = true;
    }

    protected void StopLiveUpdates()
    {
        if (!this.liveSubscribed)
        {
            return;
        }

        this.RealtimeEvents.TicketsChanged -= this.OnTicketsChanged;
        this.liveSubscribed = false;
    }

    /// <summary>
    /// Reloads the tickets after a change anywhere in the app. A change that lands while a reload is
    /// running is not dropped: the running reload goes round once more.
    /// </summary>
    protected async Task RefreshTicketsAsync()
    {
        try
        {
            await this.InvokeAsync(async () =>
            {
                if (this.isRefreshing)
                {
                    this.refreshPending = true;
                    return;
                }

                this.isRefreshing = true;
                try
                {
                    do
                    {
                        this.refreshPending = false;
                        this.Tickets = await this.TicketService.GetAllTicketsAsync();
                        this.ApplyFilters();
                    }
                    while (this.refreshPending);
                }
                finally
                {
                    this.isRefreshing = false;
                }

                this.StateHasChanged();
            });
        }
        catch
        {
            // The circuit is closing or the database is unreachable; the next change reloads again.
        }
    }

    protected void ApplyFilters() =>
        this.FilteredTickets = this.Tickets is null ? [] : TicketFilter.Apply(this.Tickets, this.Filter);

    protected void SetFilter(TicketFilterState filter)
    {
        this.Filter = filter;
        this.ApplyFilters();
    }

    // Raised on the thread that made the change.
    private void OnTicketsChanged(object? sender, EventArgs e) => _ = this.RefreshTicketsAsync();
}
