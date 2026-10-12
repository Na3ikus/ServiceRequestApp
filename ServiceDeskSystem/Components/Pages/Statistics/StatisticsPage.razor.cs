using System.Globalization;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Tickets.Models;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.Pages.Admin.Components;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Enums;
using PathSegment = ServiceDeskSystem.Components.UI.Primitives.PathSegment;

namespace ServiceDeskSystem.Components.Pages.Statistics;

/// <summary>
/// Statistics for developers and admins: ticket flow and distributions (<c>overview</c>) and team load
/// (<c>team</c>) over the last 7, 30 or 90 days in the user's zone. Charts are plain HTML, no JS.
/// </summary>
public partial class StatisticsPage : BaseComponent
{
    private const int TopTags = 8;
    private const int MinutesPerDay = 24 * 60;

    private static readonly IReadOnlyList<PathSegment> Segments = [new ("statistics")];

    // Tab and period tokens stay English, like the path (Stage 3 decision 5).
    private static readonly string[] Tabs = ["overview", "team"];

    private static readonly IReadOnlyList<(string Value, string Text)> PeriodOptions =
    [
        ("7", "7d"),
        ("30", "30d"),
        ("90", "90d"),
    ];

    // Workflow order: open statuses first, then the done ones (shown muted).
    private static readonly TicketStatus[] StatusOrder =
    [
        TicketStatus.New,
        TicketStatus.Open,
        TicketStatus.InProgress,
        TicketStatus.CodeReview,
        TicketStatus.Testing,
        TicketStatus.Done,
        TicketStatus.Resolved,
        TicketStatus.Closed,
    ];

    private static readonly TicketPriority[] PriorityOrder =
    [
        TicketPriority.Critical,
        TicketPriority.High,
        TicketPriority.Medium,
        TicketPriority.Low,
    ];

    private readonly TableSort productSort = new ("avg", descending: true);
    private readonly TableSort teamSort = new ("closed", descending: true);
    private int loadVersion;

    /// <summary>
    /// Colour of a distribution bar (and its label).
    /// </summary>
    protected enum BarTone
    {
        /// <summary>The accent: open statuses and every other category.</summary>
        Accent,

        /// <summary>Muted: done statuses.</summary>
        Muted,

        /// <summary>Danger: critical priority.</summary>
        Danger,
    }

    [Inject]
    protected ITicketStatisticsService StatisticsService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    protected bool HasAccess => this.IsAdminOrDeveloper;

    /// <summary>
    /// Gets the period length in days.
    /// </summary>
    protected int Days { get; private set; } = 30;

    protected string ActiveTab { get; private set; } = Tabs[0];

    /// <summary>
    /// Gets the last report that arrived; it stays shown while the next one loads.
    /// </summary>
    protected StatisticsReportDto? Report { get; private set; }

    protected bool IsLoading { get; private set; }

    protected static bool IsDone(TicketStatus status) =>
        status is TicketStatus.Resolved or TicketStatus.Closed or TicketStatus.Done;

    /// <summary>
    /// Bar length relative to the largest value of its chart, for a CSS percentage.
    /// </summary>
    protected static string Percent(int value, int max) =>
        max > 0 ? (value * 100.0 / max).ToString("0.#", CultureInfo.InvariantCulture) : "0";

    protected static string LabelClass(BarTone tone) => tone switch
    {
        BarTone.Muted => "text-muted",
        BarTone.Danger => "text-danger",
        _ => "text-fg",
    };

    protected static string BarClass(BarTone tone) => tone switch
    {
        BarTone.Muted => "bg-muted/60",
        BarTone.Danger => "bg-danger",
        _ => "bg-accent",
    };

    protected override async Task OnInitializedAsync()
    {
        if (!this.HasAccess)
        {
            this.Navigation.NavigateTo("/");
            return;
        }

        await this.LoadReportAsync();
    }

    protected override void OnTimeZoneChanged(object? sender, EventArgs e) => this.InvokeAsync(async () =>
    {
        // Days are the user's days, so the report is rebuilt once the browser reports its zone.
        if (this.HasAccess)
        {
            await this.LoadReportAsync();
            this.StateHasChanged();
        }
    });

    protected void SelectTab(string tab)
    {
        this.ActiveTab = tab;
    }

    protected async Task SelectPeriodAsync(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var days) || days == this.Days)
        {
            return;
        }

        this.Days = days;
        await this.LoadReportAsync();
    }

    protected void SortProducts(string column) => this.productSort.Toggle(column, descendingFirst: column != "name");

    protected void SortTeam(string column) => this.teamSort.Toggle(column, descendingFirst: column != "name");

    /// <summary>
    /// Duration in at most two units in the UI language: <c>1d 6h</c>, <c>18h</c>, <c>6h 30m</c>, <c>45m</c>;
    /// <c>—</c> without a value. Without <paramref name="withDays"/> hours are not folded into days (<c>64h</c>).
    /// </summary>
    protected string FormatDuration(TimeSpan? duration, bool withDays = true)
    {
        if (duration is not TimeSpan value)
        {
            return "—";
        }

        var totalMinutes = (long)Math.Round(Math.Max(0, value.TotalMinutes), MidpointRounding.AwayFromZero);
        var days = withDays ? totalMinutes / MinutesPerDay : 0;
        var hours = (totalMinutes - (days * MinutesPerDay)) / 60;
        var minutes = totalMinutes % 60;

        if (days > 0)
        {
            return hours > 0 ? $"{this.Unit("day", days)} {this.Unit("hour", hours)}" : this.Unit("day", days);
        }

        if (hours > 0)
        {
            return minutes > 0 ? $"{this.Unit("hour", hours)} {this.Unit("minute", minutes)}" : this.Unit("hour", hours);
        }

        return this.Unit("minute", minutes);
    }

    /// <summary>
    /// Logged time as whole hours (<c>212h</c>); less than an hour is shown in minutes.
    /// </summary>
    protected string FormatHours(int minutes) =>
        minutes is > 0 and < 60
            ? this.Unit("minute", minutes)
            : this.Unit("hour", (long)Math.Round(minutes / 60.0, MidpointRounding.AwayFromZero));

    protected string Number(int value) => value.ToString("N0", this.UiCulture);

    /// <summary>
    /// A section label with the period suffix, which stays English: <c>created · 30d</c>.
    /// </summary>
    protected string WithPeriod(string key) => $"{this.Heading(key)} · {this.Days}d";

    protected string DayTitle(DailyFlowDto day)
    {
        ArgumentNullException.ThrowIfNull(day);
        return string.Format(this.UiCulture, this.L.Translate("stats.trendDay"), day.Date.ToString("d MMM", this.UiCulture), day.Created, day.Resolved);
    }

    protected string TrendSummary(StatisticsReportDto report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return string.Format(this.UiCulture, this.L.Translate("stats.trendSummary"), report.Days, report.Period.Created, report.Period.Resolved);
    }

    /// <summary>
    /// Five date labels under the trend: the first day, three evenly spaced ones and <c>today</c>.
    /// </summary>
    protected IEnumerable<string> AxisLabels(StatisticsReportDto report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var last = report.Trend.Count - 1;
        return Enumerable.Range(0, 5)
            .Select(i => (int)Math.Round(last * i / 4.0, MidpointRounding.AwayFromZero))
            .Distinct()
            .Select(i => i == last ? this.Heading("stats.today") : report.Trend[i].Date.ToString("d MMM", this.UiCulture));
    }

    protected IEnumerable<DistributionRow> TypeRows(StatisticsSnapshotDto snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var total = snapshot.ByType.Values.Sum();
        return snapshot.ByType
            .OrderByDescending(t => t.Value)
            .ThenBy(t => t.Key)
            .Select(t => new DistributionRow(
                t.Key.ToString(),
                this.GetTicketTypeText(t.Key),
                t.Value,
                BarTone.Accent,
                total > 0 ? (int)Math.Round(t.Value * 100.0 / total, MidpointRounding.AwayFromZero) : 0));
    }

    protected IEnumerable<ProductStatisticsDto> SortedProducts(StatisticsReportDto report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var rows = report.Products;
        var ordered = this.productSort.Column switch
        {
            "name" => this.productSort.Apply(rows, p => p.Name, StringComparer.CurrentCultureIgnoreCase),
            "open" => this.productSort.Apply(rows, p => p.Open),
            "created" => this.productSort.Apply(rows, p => p.Created),
            "resolved" => this.productSort.Apply(rows, p => p.Resolved),

            // Products without a resolution time sort below the fastest one.
            _ => this.productSort.Apply(rows, p => p.AverageResolution ?? TimeSpan.MinValue),
        };

        return ordered.ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase);
    }

    protected IEnumerable<DeveloperStatisticsDto> SortedTeam(StatisticsReportDto report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var rows = report.Team;
        var ordered = this.teamSort.Column switch
        {
            "name" => this.teamSort.Apply(rows, r => r.DisplayName, StringComparer.CurrentCultureIgnoreCase),
            "inprogress" => this.teamSort.Apply(rows, r => r.InProgress),
            "active" => this.teamSort.Apply(rows, r => r.Active),
            "time" => this.teamSort.Apply(rows, r => r.MinutesInPeriod),
            "avg" => this.teamSort.Apply(rows, r => r.AverageMinutesPerTicket ?? -1),
            "workload" => this.teamSort.Apply(rows, r => r.Workload),
            _ => this.teamSort.Apply(rows, r => r.ClosedInPeriod),
        };

        return ordered.ThenByDescending(r => r.Workload).ThenBy(r => r.DisplayName, StringComparer.CurrentCultureIgnoreCase);
    }

    private string Unit(string unit, long value) =>
        string.Format(this.UiCulture, this.L.Translate($"stats.unit.{unit}"), value);

    /// <summary>
    /// Loads the report for the selected period. Each load is numbered and only the latest one is applied,
    /// so a slow earlier response cannot replace a newer period's report.
    /// </summary>
    private async Task LoadReportAsync()
    {
        var version = ++this.loadVersion;
        this.IsLoading = true;
        try
        {
            var report = await this.StatisticsService.GetStatisticsReportAsync(this.Days, this.UserZone.Zone);
            if (version == this.loadVersion)
            {
                this.Report = report;
            }
        }
        catch (Exception ex)
        {
            if (version == this.loadVersion)
            {
                await this.ToastService.ShowToastAsync($"{this.L.Translate("stats.loadFailed")}: {ex.Message}", ToastType.Error);
            }
        }
        finally
        {
            if (version == this.loadVersion)
            {
                this.IsLoading = false;
            }
        }
    }

    /// <summary>
    /// One line of a distribution: label | bar | value (and an optional share in percent).
    /// </summary>
    protected sealed class DistributionRow
    {
        public DistributionRow(string key, string label, int value, BarTone tone, int? share = null)
        {
            this.Key = key;
            this.Label = label;
            this.Value = value;
            this.Tone = tone;
            this.Share = share;
        }

        /// <summary>
        /// Gets the stable key used in the <c>data-*</c> attributes (enum name or tag id).
        /// </summary>
        public string Key { get; }

        public string Label { get; }

        public int Value { get; }

        public BarTone Tone { get; }

        /// <summary>
        /// Gets the share of the list's total in percent, shown muted after the value; <c>null</c> to hide it.
        /// </summary>
        public int? Share { get; }
    }
}
