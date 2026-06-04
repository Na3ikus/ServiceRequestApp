using System.Security.Cryptography;
using System.Threading;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ServiceDeskSystem.Components.Features;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Developer;

/// <summary>
/// Developer dashboard page component.
/// </summary>
public partial class DeveloperDashboard : BaseComponent
{
    private static readonly TimeSpan BaseRefreshInterval = TimeSpan.FromSeconds(12);
    private static readonly TimeSpan MaxRefreshJitter = TimeSpan.FromSeconds(3);

    private CancellationTokenSource? refreshLoopCts;
    private Task? refreshLoopTask;
    private bool isRefreshing;
    private bool authRestored;

    [Inject]
    private ITicketService TicketService { get; set; } = null!;

    [Inject]
    private ITicketStatisticsService TicketStatisticsService { get; set; } = null!;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    private List<Ticket>? tickets { get; set; }

    private int assignedCount { get; set; }

    private int inProgressCount { get; set; }

    private int completedCount { get; set; }

    private string viewMode { get; set; } = "Table";

    private int CurrentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    private UserRole? CurrentUserRole => this.AuthService.CurrentUser?.Role;

    private bool IsDeveloper => this.CurrentUserRole == UserRole.Developer ||
                                this.CurrentUserRole == UserRole.Admin;

    private bool IsAdmin => this.CurrentUserRole == UserRole.Admin;
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
            this.refreshLoopCts?.Cancel();
            this.refreshLoopCts?.Dispose();
        }

        base.Dispose(disposing);
    }

    private async Task LoadDataAsync()
    {
        if (!this.IsDeveloper || this.CurrentUserId == 0)
        {
            return;
        }

        var ticketsTask = this.TicketService.GetDeveloperTicketsAsync(this.CurrentUserId);
        var statsTask = this.TicketStatisticsService.GetDeveloperDashboardStatsAsync(this.CurrentUserId);

        await Task.WhenAll(ticketsTask, statsTask);

        this.tickets = await ticketsTask;
        var stats = await statsTask;

        this.assignedCount = stats.AssignedCount;
        this.inProgressCount = stats.InProgressCount;
        this.completedCount = stats.CompletedCount;
    }

    private void StartAutoRefresh()
    {
        if (this.refreshLoopTask is not null)
        {
            return;
        }

        this.refreshLoopCts = new CancellationTokenSource();
        this.refreshLoopTask = this.RunRefreshLoopAsync(this.refreshLoopCts.Token);
    }

    private async Task RunRefreshLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var delay = BaseRefreshInterval + TimeSpan.FromMilliseconds(milliseconds: RandomNumberGenerator.GetInt32(0, (int)MaxRefreshJitter.TotalMilliseconds + 1));
                await Task.Delay(delay, cancellationToken);

                if (!await this.IsPageVisibleAsync())
                {
                    continue;
                }

                await this.RefreshAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on component disposal.
        }
    }

    private async Task<bool> IsPageVisibleAsync()
    {
        try
        {
            return await this.JS.InvokeAsync<bool>("dashboardVisibility.isVisible");
        }
        catch (JSException)
        {
            return true;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
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

    private async void OnAuthStateChanged(object? sender, EventArgs e)
    {
        await this.LoadDataAsync();
        await this.InvokeAsync(this.StateHasChanged);
    }

    private void SetViewMode(string mode)
    {
        this.viewMode = mode;
        this.StateHasChanged();
    }

    private async Task HandleKanbanStatusChangedAsync((int TicketId, TicketStatus NewStatus) args)
    {
        var ticket = this.tickets?.FirstOrDefault(t => t.Id == args.TicketId);
        if (ticket is null)
        {
            return;
        }

        if (!this.IsAdmin && ticket.DeveloperId != this.CurrentUserId)
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
}
