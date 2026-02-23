using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Components.Common;
using ServiceDeskSystem.Components.Common.Base;

namespace ServiceDeskSystem.Components.Pages.Statistics;

/// <summary>
/// Statistics dashboard page — accessible to Developer and Admin only.
/// </summary>
#pragma warning disable CA1724 // The type name Statistics conflicts with the namespace name
public partial class Statistics : BaseComponent
#pragma warning restore CA1724
{
    // Status display order and colors (static first per SA1204)
    private static readonly (string Status, string Color)[] StatusOrder =
    [
        ("New",         "from-purple-500 to-purple-600"),
        ("Open",        "from-blue-500 to-blue-600"),
        ("In Progress", "from-amber-500 to-orange-500"),
        ("Testing",     "from-cyan-500 to-cyan-600"),
        ("Code Review", "from-indigo-500 to-indigo-600"),
        ("Done",        "from-emerald-500 to-green-500"),
        ("Resolved",    "from-green-500 to-green-600"),
        ("Closed",      "from-gray-400 to-gray-500"),
    ];

    private static readonly (string Priority, string Color)[] PriorityOrder =
    [
        ("Critical", "from-red-500 to-rose-600"),
        ("High",     "from-orange-500 to-orange-600"),
        ("Medium",   "from-yellow-500 to-yellow-600"),
        ("Low",      "from-green-500 to-emerald-500"),
    ];

    // Fields
    private bool isLoading = true;

    private int totalTickets;

    private int openTickets;

    private int criticalTickets;

    private int resolvedTickets;

    private Dictionary<string, int> byStatus = new Dictionary<string, int>();

    private Dictionary<string, int> byPriority = new Dictionary<string, int>();

    private List<(string Login, int Count)> topDevs = [];

    // Injected services (properties after fields per SA1201)
    [Inject]
    private ITicketService TicketService { get; set; } = null!;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private string CurrentUserRole => this.AuthService.CurrentUser?.Role ?? string.Empty;

    private bool HasAccess => string.Equals(this.CurrentUserRole, "Developer", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(this.CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase);

    /// <summary>Percentage for bar chart width (min 2% so bar is always visible).</summary>
    private static int Pct(int value, int total) =>
        total == 0 ? 0 : Math.Max(2, (int)Math.Round(value * 100.0 / total));

    protected override async Task OnInitializedAsync()
    {
        if (!this.HasAccess)
        {
            this.Navigation.NavigateTo("/");
            return;
        }

        await this.LoadDataAsync();
        this.isLoading = false;
    }

    private async Task LoadDataAsync()
    {
        this.byStatus = await this.TicketService.GetTicketCountByStatusAsync();
        this.byPriority = await this.TicketService.GetTicketCountByPriorityAsync();
        this.topDevs = await this.TicketService.GetTopDevelopersAsync(5);

        this.totalTickets = this.byStatus.Values.Sum();
        this.openTickets = this.byStatus.TryGetValue("Open", out var o) ? o : 0;
        this.criticalTickets = this.byPriority.TryGetValue("Critical", out var c) ? c : 0;
        this.resolvedTickets = (this.byStatus.TryGetValue("Resolved", out var r) ? r : 0)
                             + (this.byStatus.TryGetValue("Closed", out var cl) ? cl : 0)
                             + (this.byStatus.TryGetValue("Done", out var d) ? d : 0);
    }
}
