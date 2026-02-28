using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Tickets.Interfaces;
using ServiceDeskSystem.Components.Features;
using ServiceDeskSystem.Components.UI.Base;

namespace ServiceDeskSystem.Components.Pages.Statistics;

/// <summary>
/// Statistics dashboard page — accessible to Developer and Admin only.
/// </summary>
#pragma warning disable CA1724 // The type name Statistics conflicts with the namespace name
public partial class Statistics : BaseComponent
#pragma warning restore CA1724
{
    // Status display order and colors (static first per SA1204)
    private static readonly (string Status, string Color, string HexColor)[] StatusOrder =
    [
        ("New",         "from-purple-500 to-purple-600", "#a855f7"),
        ("Open",        "from-blue-500 to-blue-600", "#3b82f6"),
        ("In Progress", "from-amber-500 to-orange-500", "#f59e0b"),
        ("Testing",     "from-cyan-500 to-cyan-600", "#06b6d4"),
        ("Code Review", "from-indigo-500 to-indigo-600", "#6366f1"),
        ("Done",        "from-emerald-500 to-green-500", "#10b981"),
        ("Resolved",    "from-green-500 to-green-600", "#22c55e"),
        ("Closed",      "from-gray-400 to-gray-500", "#9ca3af"),
    ];

    private static readonly (string Priority, string Color, string HexColor)[] PriorityOrder =
    [
        ("Critical", "from-red-500 to-rose-600", "#ef4444"),
        ("High",     "from-orange-500 to-orange-600", "#f97316"),
        ("Medium",   "from-yellow-500 to-yellow-600", "#eab308"),
        ("Low",      "from-green-500 to-emerald-500", "#22c55e"),
    ];

    // Fields
    private bool isLoading = true;

    private bool shouldRenderCharts;

    private int totalTickets;

    private int openTickets;

    private int criticalTickets;

    private int resolvedTickets;

    private Dictionary<string, int> byStatus = new Dictionary<string, int>();

    private Dictionary<string, int> byPriority = new Dictionary<string, int>();

    private List<(string Login, int Count)> topDevs = [];

    // Injected services (properties after fields per SA1201)
    [Inject]
    private ITicketStatisticsService TicketStatisticsService { get; set; } = null!;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    private string CurrentUserRole => this.AuthService.CurrentUser?.Role ?? string.Empty;

    private bool HasAccess => string.Equals(this.CurrentUserRole, "Developer", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(this.CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase);

    protected override async Task OnInitializedAsync()
    {
        if (!this.HasAccess)
        {
            this.Navigation.NavigateTo("/");
            return;
        }

        await this.LoadDataAsync();
        this.isLoading = false;
        this.shouldRenderCharts = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (this.shouldRenderCharts)
        {
            this.shouldRenderCharts = false;

            await this.JS.InvokeVoidAsync("chartInterop.animateCountUp", "totalCount", 0, this.totalTickets, 1000);
            await this.JS.InvokeVoidAsync("chartInterop.animateCountUp", "openCount", 0, this.openTickets, 1000);
            await this.JS.InvokeVoidAsync("chartInterop.animateCountUp", "criticalCount", 0, this.criticalTickets, 1000);
            await this.JS.InvokeVoidAsync("chartInterop.animateCountUp", "resolvedCount", 0, this.resolvedTickets, 1000);

            var statusLabels = StatusOrder.Select(s => this.GetStatusText(s.Status)).ToArray();
            var statusData = StatusOrder.Select(s => this.byStatus.TryGetValue(s.Status, out var v) ? v : 0).ToArray();
            var statusColors = StatusOrder.Select(s => s.HexColor).ToArray();

            await this.JS.InvokeVoidAsync("chartInterop.renderStatusChart", "statusChart", statusLabels, statusData, statusColors);

            var priorityLabels = PriorityOrder.Select(p => this.GetPriorityText(p.Priority)).ToArray();
            var priorityData = PriorityOrder.Select(p => this.byPriority.TryGetValue(p.Priority, out var v) ? v : 0).ToArray();
            var priorityColors = PriorityOrder.Select(p => p.HexColor).ToArray();

            await this.JS.InvokeVoidAsync("chartInterop.renderPriorityChart", "priorityChart", priorityLabels, priorityData, priorityColors);
        }
    }

    /// <summary>Percentage for bar chart width (min 2% so bar is always visible).</summary>
    private static int Pct(int value, int total) =>
        total == 0 ? 0 : Math.Max(2, (int)Math.Round(value * 100.0 / total));

    private async Task LoadDataAsync()
    {
        this.byStatus = await this.TicketStatisticsService.GetTicketCountByStatusAsync();
        this.byPriority = await this.TicketStatisticsService.GetTicketCountByPriorityAsync();
        this.topDevs = await this.TicketStatisticsService.GetTopDevelopersAsync(5);

        this.totalTickets = this.byStatus.Values.Sum();
        this.openTickets = this.byStatus.TryGetValue("Open", out var o) ? o : 0;
        this.criticalTickets = this.byPriority.TryGetValue("Critical", out var c) ? c : 0;
        this.resolvedTickets = (this.byStatus.TryGetValue("Resolved", out var r) ? r : 0)
                             + (this.byStatus.TryGetValue("Closed", out var cl) ? cl : 0)
                             + (this.byStatus.TryGetValue("Done", out var d) ? d : 0);
    }
}
