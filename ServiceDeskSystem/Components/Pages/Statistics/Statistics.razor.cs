using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Tickets.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Statistics;

/// <summary>
/// Active tab in the analytics dashboard.
/// </summary>
public enum AnalyticsTab
{
    /// <summary>Overview tab with general metrics.</summary>
    Overview,

    /// <summary>Extended tab with detailed trends and workloads.</summary>
    Extended,

    /// <summary>Efficiency tab for employee performance.</summary>
    Efficiency,
}

/// <summary>
/// Statistics and advanced analytics dashboard page — accessible to Developer and Admin only.
/// </summary>
public partial class Statistics : BaseComponent
{
    private static readonly (TicketStatus Status, string Color, string HexColor)[] StatusOrder =
    [
        (TicketStatus.New, "from-purple-500 to-purple-600", "#a855f7"),
        (TicketStatus.Open, "from-blue-500 to-blue-600", "#3b82f6"),
        (TicketStatus.InProgress, "from-amber-500 to-orange-500", "#f59e0b"),
        (TicketStatus.Testing, "from-cyan-500 to-cyan-600", "#06b6d4"),
        (TicketStatus.CodeReview, "from-indigo-500 to-indigo-600", "#6366f1"),
        (TicketStatus.Done, "from-emerald-500 to-green-500", "#10b981"),
        (TicketStatus.Resolved, "from-green-500 to-green-600", "#22c55e"),
        (TicketStatus.Closed, "from-gray-400 to-gray-500", "#9ca3af"),
    ];

    private static readonly (TicketPriority Priority, string Color, string HexColor)[] PriorityOrder =
    [
        (TicketPriority.Critical, "from-red-500 to-rose-600", "#ef4444"),
        (TicketPriority.High, "from-orange-500 to-orange-600", "#f97316"),
        (TicketPriority.Medium, "from-yellow-500 to-yellow-600", "#eab308"),
        (TicketPriority.Low, "from-green-500 to-emerald-500", "#22c55e"),
    ];

    private static readonly (TicketType Type, string Color, string HexColor)[] TypeOrder =
    [
        (TicketType.Bug, "from-red-500 to-rose-600", "#ef4444"),
        (TicketType.Support, "from-blue-500 to-cyan-600", "#3b82f6"),
        (TicketType.Consultation, "from-amber-500 to-orange-500", "#f59e0b"),
        (TicketType.Project, "from-purple-500 to-indigo-600", "#8b5cf6"),
    ];

    [Inject]
    protected ITicketStatisticsService TicketStatisticsService { get; set; } = null!;

    [Inject]
    protected IJSRuntime JS { get; set; } = null!;

    protected bool IsLoading { get; set; } = true;

    protected bool ShouldRenderCharts { get; set; }

    protected AnalyticsTab CurrentTab { get; set; } = AnalyticsTab.Overview;

    protected int SelectedDays { get; set; } = 30;

    protected int TotalTickets { get; set; }

    protected int OpenTickets { get; set; }

    protected int CriticalTickets { get; set; }

    protected int ResolvedTickets { get; set; }

    protected Dictionary<string, int> ByStatus { get; set; } = new ();

    protected Dictionary<string, int> ByPriority { get; set; } = new ();

    protected Dictionary<string, int> ByType { get; set; } = new ();

    protected List<(string Login, int Count)> TopDevs { get; set; } = [];

    protected ExtendedAnalyticsDto? ExtendedData { get; set; }

    protected List<EmployeeEfficiencyDto>? EfficiencyData { get; set; }

    protected UserRole? CurrentUserRole => this.AuthService.CurrentUser?.Role;

    protected bool HasAccess => this.CurrentUserRole == UserRole.Developer
                           || this.CurrentUserRole == UserRole.Admin;

    protected static int Pct(int value, int total) =>
        total == 0 ? 0 : Math.Max(2, (int)Math.Round(value * 100.0 / total));

    protected override async Task OnInitializedAsync()
    {
        if (!this.HasAccess)
        {
            this.Navigation.NavigateTo("/");
            return;
        }

        await this.LoadDataAsync();
        this.IsLoading = false;
        this.ShouldRenderCharts = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (this.ShouldRenderCharts && !this.IsLoading)
        {
            this.ShouldRenderCharts = false;

            if (this.CurrentTab == AnalyticsTab.Overview)
            {
                await this.JS.InvokeVoidAsync("chartInterop.animateCountUp", "totalCount", 0, this.TotalTickets, 800);
                await this.JS.InvokeVoidAsync("chartInterop.animateCountUp", "openCount", 0, this.OpenTickets, 800);
                await this.JS.InvokeVoidAsync("chartInterop.animateCountUp", "criticalCount", 0, this.CriticalTickets, 800);
                await this.JS.InvokeVoidAsync("chartInterop.animateCountUp", "resolvedCount", 0, this.ResolvedTickets, 800);

                var statusLabels = StatusOrder.Select(s => this.GetStatusText(s.Status)).ToArray();
                var statusData = StatusOrder.Select(s => this.ByStatus.TryGetValue(s.Status.ToString(), out var v) ? v : 0).ToArray();
                var statusColors = StatusOrder.Select(s => s.HexColor).ToArray();
                await this.JS.InvokeVoidAsync("chartInterop.renderStatusChart", "statusChart", statusLabels, statusData, statusColors);

                var priorityLabels = PriorityOrder.Select(p => this.GetPriorityText(p.Priority)).ToArray();
                var priorityData = PriorityOrder.Select(p => this.ByPriority.TryGetValue(p.Priority.ToString(), out var v) ? v : 0).ToArray();
                var priorityColors = PriorityOrder.Select(p => p.HexColor).ToArray();
                await this.JS.InvokeVoidAsync("chartInterop.renderPriorityChart", "priorityChart", priorityLabels, priorityData, priorityColors);

                var typeLabels = TypeOrder.Select(t => this.GetTicketTypeText(t.Type)).ToArray();
                var typeData = TypeOrder.Select(t => this.ByType.TryGetValue(t.Type.ToString(), out var v) ? v : 0).ToArray();
                var typeColors = TypeOrder.Select(t => t.HexColor).ToArray();
                await this.JS.InvokeVoidAsync("chartInterop.renderTypeChart", "typeChart", typeLabels, typeData, typeColors);
            }
            else if (this.CurrentTab == AnalyticsTab.Extended && this.ExtendedData is not null)
            {
                var trendLabels = this.ExtendedData.Trends.Select(t => t.DateLabel).ToArray();
                var trendCreated = this.ExtendedData.Trends.Select(t => t.CreatedCount).ToArray();
                var trendResolved = this.ExtendedData.Trends.Select(t => t.ResolvedCount).ToArray();
                var trendCreatedLabel = this.L.Translate("analytics.chartCreated");
                var trendResolvedLabel = this.L.Translate("analytics.chartResolved");
                await this.JS.InvokeVoidAsync("chartInterop.renderTrendChart", "trendChart", trendLabels, trendCreated, trendResolved, trendCreatedLabel, trendResolvedLabel);

                if (this.ExtendedData.TagDistributions.Count > 0)
                {
                    var tagLabels = this.ExtendedData.TagDistributions.Select(t => t.TagName).ToArray();
                    var tagCounts = this.ExtendedData.TagDistributions.Select(t => t.TicketCount).ToArray();
                    var tagColors = this.ExtendedData.TagDistributions.Select(t => string.IsNullOrWhiteSpace(t.Color) ? "#3b82f6" : t.Color).ToArray();
                    await this.JS.InvokeVoidAsync("chartInterop.renderTagDistributionChart", "tagsChart", tagLabels, tagCounts, tagColors);
                }

                var devLabels = this.ExtendedData.DeveloperWorkloads.Select(d => d.Login).ToArray();
                var inProgressData = this.ExtendedData.DeveloperWorkloads.Select(d => d.InProgressCount).ToArray();
                var assignedData = this.ExtendedData.DeveloperWorkloads.Select(d => Math.Max(0, d.AssignedCount - d.InProgressCount)).ToArray();
                var completedData = this.ExtendedData.DeveloperWorkloads.Select(d => d.ResolvedCount).ToArray();
                var inProgressLabel = this.L.Translate("analytics.chartInProgress");
                var assignedLabel = this.L.Translate("analytics.chartAssigned");
                var completedLabel = this.L.Translate("analytics.chartCompleted");
                await this.JS.InvokeVoidAsync("chartInterop.renderWorkloadChart", "workloadChart", devLabels, inProgressData, assignedData, completedData, inProgressLabel, assignedLabel, completedLabel);

                var prodLabels = this.ExtendedData.ProductPerformances.Select(p => p.ProductName).ToArray();
                var prodHours = this.ExtendedData.ProductPerformances.Select(p => p.AvgResolutionHours).ToArray();
                var prodResolutionLabel = this.L.Translate("analytics.chartAvgResolutionHours");
                await this.JS.InvokeVoidAsync("chartInterop.renderProductPerformanceChart", "productChart", prodLabels, prodHours, prodResolutionLabel);
            }
        }
    }

    protected async Task SwitchTab(AnalyticsTab tab)
    {
        if (this.CurrentTab == tab)
        {
            return;
        }

        this.CurrentTab = tab;
        this.ShouldRenderCharts = true;

        if (this.CurrentTab == AnalyticsTab.Extended && this.ExtendedData is null)
        {
            await this.LoadExtendedDataAsync();
        }
        else if (this.CurrentTab == AnalyticsTab.Efficiency && this.EfficiencyData is null)
        {
            await this.LoadEfficiencyDataAsync();
        }

        this.StateHasChanged();
    }

    protected async Task SetDaysFilter(int days)
    {
        if (this.SelectedDays == days)
        {
            return;
        }

        this.SelectedDays = days;
        if (this.CurrentTab == AnalyticsTab.Extended)
        {
            await this.LoadExtendedDataAsync();
        }
        else if (this.CurrentTab == AnalyticsTab.Efficiency)
        {
            await this.LoadEfficiencyDataAsync();
        }
        
        this.ShouldRenderCharts = true;
        this.StateHasChanged();
    }

    private async Task LoadDataAsync()
    {
        this.ByStatus = await this.TicketStatisticsService.GetTicketCountByStatusAsync();
        this.ByPriority = await this.TicketStatisticsService.GetTicketCountByPriorityAsync();
        this.ByType = await this.TicketStatisticsService.GetTicketCountByTypeAsync();
        this.TopDevs = await this.TicketStatisticsService.GetTopDevelopersAsync(5);

        this.TotalTickets = this.ByStatus.Values.Sum();
        this.OpenTickets = this.ByStatus.TryGetValue(TicketStatus.Open.ToString(), out var o) ? o : 0;
        this.CriticalTickets = this.ByPriority.TryGetValue(TicketPriority.Critical.ToString(), out var c) ? c : 0;
        this.ResolvedTickets = (this.ByStatus.TryGetValue(TicketStatus.Resolved.ToString(), out var r) ? r : 0)
                             + (this.ByStatus.TryGetValue(TicketStatus.Closed.ToString(), out var cl) ? cl : 0)
                             + (this.ByStatus.TryGetValue(TicketStatus.Done.ToString(), out var d) ? d : 0);

        await this.LoadExtendedDataAsync();
    }

    private async Task LoadExtendedDataAsync()
    {
        this.ExtendedData = await this.TicketStatisticsService.GetExtendedAnalyticsAsync(this.SelectedDays);
    }

    private async Task LoadEfficiencyDataAsync()
    {
        var efficiency = await this.TicketStatisticsService.GetEmployeeEfficiencyAsync(this.SelectedDays);
        this.EfficiencyData = efficiency.ToList();
    }
}
