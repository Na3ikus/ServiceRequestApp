using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Tickets.Models;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.Pages.Statistics;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Pages;

[TestFixture]
public class StatisticsPageTests
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");
    private static readonly DateOnly Today = new (2026, 10, 12);

    private ComponentTestHost _host;
    private Mock<ITicketStatisticsService> _stats;
    private StatisticsReportDto _report;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();
        _host.TimeZone.Set("Pacific/Kiritimati");
        _host.Localization.Setup(l => l.Translate("stats.unit.day")).Returns("{0}d");
        _host.Localization.Setup(l => l.Translate("stats.unit.hour")).Returns("{0}h");
        _host.Localization.Setup(l => l.Translate("stats.unit.minute")).Returns("{0}m");
        _host.Localization.Setup(l => l.Translate("stats.trendDay")).Returns("{0}: created {1}, resolved {2}");
        _host.Localization.Setup(l => l.Translate("stats.trendSummary")).Returns("Last {0} days: {1} created, {2} resolved");

        _report = Report();
        _stats = new Mock<ITicketStatisticsService>();
        _stats.Setup(s => s.GetStatisticsReportAsync(It.IsAny<int>(), It.IsAny<TimeZoneInfo>())).ReturnsAsync(() => _report);
        _host.Context.Services.AddSingleton(_stats.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private static StatisticsReportDto Report(
        int days = 7,
        int created = 12,
        int criticalOpen = 0,
        IReadOnlyList<DailyFlowDto>? trend = null,
        IReadOnlyList<TagCountDto>? tags = null,
        IReadOnlyList<ProductStatisticsDto>? products = null,
        IReadOnlyList<DeveloperStatisticsDto>? team = null)
    {
        var from = Today.AddDays(-(days - 1));
        trend ??= Enumerable.Range(0, days).Select(i => new DailyFlowDto(from.AddDays(i), i % 3, i % 2)).ToList();
        var snapshot = new StatisticsSnapshotDto(
            42,
            criticalOpen,
            Enum.GetValues<TicketStatus>().ToDictionary(s => s, s => s switch
            {
                TicketStatus.Open => 18,
                TicketStatus.Done => 29,
                _ => 2,
            }),
            new Dictionary<TicketPriority, int>
            {
                [TicketPriority.Critical] = criticalOpen,
                [TicketPriority.High] = 11,
                [TicketPriority.Medium] = 19,
                [TicketPriority.Low] = 9,
            },
            new Dictionary<TicketType, int>
            {
                [TicketType.Bug] = 17,
                [TicketType.Support] = 14,
                [TicketType.Project] = 7,
                [TicketType.Consultation] = 4,
            },
            tags ?? [new (1, "backend", 15), new (2, "ui", 11)]);

        return new StatisticsReportDto(
            days,
            from,
            Today,
            snapshot,
            new StatisticsPeriodDto(created, 11, TimeSpan.FromHours(30), 92),
            trend,
            products ??
            [
                new (1, "Billing", 14, 21, 17, TimeSpan.FromHours(52)),
                new (2, "Web portal", 12, 17, 18, TimeSpan.FromHours(18)),
                new (3, "Mobile", 11, 3, 0, null),
            ],
            team ??
            [
                new (1, "okoval", "Olena Koval", 3, 7, 19, 3840),
                new (2, "dmelnyk", "Dmytro Melnyk", 2, 5, 14, 3060),
                new (3, "tboyko", "Taras Boyko", 0, 0, 0, 0),
            ]);
    }

    private IRenderedComponent<TestStatisticsPage> Render() => _host.Context.RenderComponent<TestStatisticsPage>();

    private static IElement Option(IRenderedComponent<TestStatisticsPage> cut, string text) =>
        cut.FindAll("[role=radiogroup][aria-label='stats.period'] button").Single(b => b.TextContent.Trim() == text);

    private static string Figure(IRenderedComponent<TestStatisticsPage> cut, string key) =>
        cut.Find($"[data-figure={key}] p").TextContent.Trim();

    private static List<string> Column(IRenderedComponent<TestStatisticsPage> cut, string table, int index) =>
        cut.FindAll($"[data-table={table}] tbody tr").Select(r => r.Children[index].TextContent.Trim()).ToList();

    [Test]
    public void Users_without_developer_or_admin_role_are_redirected_and_nothing_loads()
    {
        _host.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = 5, Login = "client", Role = UserRole.User });

        var cut = Render();

        _host.Context.Services.GetRequiredService<FakeNavigationManager>().Uri.Should().Be("http://localhost/");
        cut.FindAll("h1").Should().BeEmpty();
        _stats.Verify(s => s.GetStatisticsReportAsync(It.IsAny<int>(), It.IsAny<TimeZoneInfo>()), Times.Never);
    }

    [Test]
    public void Header_shows_the_path_the_period_and_the_tabs()
    {
        var cut = Render();

        cut.Find("h1").TextContent.Replace(" ", string.Empty).Should().Be("~/statistics");
        Option(cut, "30d").GetAttribute("aria-checked").Should().Be("true");
        cut.FindAll("[data-tab]").Select(t => t.TextContent.Trim()).Should().Equal("overview", "team");
        cut.Find("[data-tab=overview]").GetAttribute("aria-current").Should().Be("true");
        cut.FindAll("canvas").Should().BeEmpty();
        _stats.Verify(s => s.GetStatisticsReportAsync(30, _host.TimeZone.Zone), Times.Once);
    }

    [Test]
    public void Figures_show_the_report_with_the_period_suffix()
    {
        _report = Report(days: 30);

        var cut = Render();

        Figure(cut, "open").Should().Be("42");
        Figure(cut, "created").Should().Be("12");
        Figure(cut, "resolved").Should().Be("11");
        Figure(cut, "resolution").Should().Be("1d 6h");
        Figure(cut, "ratio").Should().Be("92%");
        cut.Find("[data-figure=created] p:last-child").TextContent.Should().Be("# stats.created · 30d");
    }

    [TestCase(0, false)]
    [TestCase(3, true)]
    public void Critical_open_is_in_danger_only_when_above_zero(int critical, bool danger)
    {
        _report = Report(criticalOpen: critical);

        var cut = Render();

        cut.Find("[data-figure=critical] p").ClassList.Contains("text-danger").Should().Be(danger);
    }

    [Test]
    public void Ratio_and_resolution_show_a_dash_without_data()
    {
        _report = Report() with { Period = new StatisticsPeriodDto(0, 0, null, null) };

        var cut = Render();

        Figure(cut, "resolution").Should().Be("—");
        Figure(cut, "ratio").Should().Be("—");
    }

    [Test]
    public void Trend_has_one_pair_per_day_relative_to_the_busiest_day()
    {
        var from = Today.AddDays(-6);
        _report = Report(trend:
        [
            new (from, 4, 2), new (from.AddDays(1), 0, 0), new (from.AddDays(2), 1, 3), new (from.AddDays(3), 0, 0),
            new (from.AddDays(4), 0, 0), new (from.AddDays(5), 0, 0), new (Today, 2, 1),
        ]);

        var cut = Render();

        var days = cut.FindAll("[data-trend] [data-day]").ToList();
        days.Should().HaveCount(7);
        days[0].QuerySelector("[data-bar=created]")!.GetAttribute("style").Should().Be("height: 100%");
        days[0].QuerySelector("[data-bar=resolved]")!.GetAttribute("style").Should().Be("height: 50%");
        days[2].QuerySelector("[data-bar=resolved]")!.GetAttribute("style").Should().Be("height: 75%");
        days[1].QuerySelector("[data-bar=created]")!.GetAttribute("style").Should().Be("height: 0%");
        days[1].QuerySelector("[data-bar=created]")!.ClassList.Should().Contain(["min-h-px", "bg-accent/35"]);
        days[0].GetAttribute("title").Should().Be($"{from.ToString("d MMM", English)}: created 4, resolved 2");

        var chart = cut.Find("[data-trend]");
        chart.GetAttribute("role").Should().Be("img");
        chart.GetAttribute("aria-label").Should().Be("Last 7 days: 12 created, 11 resolved");
        var axis = cut.FindAll("[data-axis] span").Select(s => s.TextContent).ToList();
        axis.Should().HaveCount(5);
        axis[0].Should().Be(from.ToString("d MMM", English));
        axis[^1].Should().Be("stats.today");
    }

    [Test]
    public void Ninety_days_keep_one_pair_per_day()
    {
        _report = Report(days: 90);

        var cut = Render();

        cut.FindAll("[data-trend] [data-day]").Should().HaveCount(90);
        cut.Find("[data-trend]").ClassList.Should().Contain("gap-px");
    }

    [Test]
    public void Distributions_mute_done_statuses_mark_critical_and_keep_tags_single_hue()
    {
        _report = Report(criticalOpen: 3);

        var cut = Render();

        var status = cut.Find("[data-dist=status]");
        status.QuerySelectorAll("[data-label]").Select(l => l.GetAttribute("data-label")).Should().Equal(
            "New", "Open", "InProgress", "CodeReview", "Testing", "Done", "Resolved", "Closed");
        status.QuerySelector("[data-label=Done]")!.ClassList.Should().Contain("text-muted");
        status.QuerySelector("[data-bar=Done]")!.ClassList.Should().Contain("bg-muted/60");
        status.QuerySelector("[data-bar=Done]")!.GetAttribute("style").Should().Be("width: 100%");
        status.QuerySelector("[data-bar=Open]")!.ClassList.Should().Contain("bg-accent");
        status.QuerySelector("[data-label=Open]")!.TextContent.Should().Be("status.open");

        var priority = cut.Find("[data-dist=priority]");
        priority.QuerySelector("[data-label=Critical]")!.ClassList.Should().Contain("text-danger");
        priority.QuerySelector("[data-bar=Critical]")!.ClassList.Should().Contain("bg-danger");
        priority.QuerySelector("[data-bar=High]")!.ClassList.Should().Contain("bg-accent");

        cut.Find("[data-dist=type] [data-value=Bug]").TextContent.Should().Be("1740%");
        cut.Find("[data-dist=type] [data-label]").GetAttribute("data-label").Should().Be("Bug");

        var tagBars = cut.FindAll("[data-dist=tags] [data-bar]");
        tagBars.Should().HaveCount(2).And.AllSatisfy(b => b.ClassList.Should().Contain("bg-accent"));
        tagBars.Should().AllSatisfy(b => b.GetAttribute("style").Should().NotContain("background"));
    }

    [Test]
    public void Tags_show_the_top_eight_or_no_data()
    {
        _report = Report(tags: Enumerable.Range(1, 10).Select(i => new TagCountDto(i, $"tag{i}", 20 - i)).ToList());
        var cut = Render();
        cut.FindAll("[data-dist=tags] [data-label]").Should().HaveCount(8);

        _report = Report(tags: []);
        cut = Render();
        cut.Find("[data-dist=tags] [data-no-data]").TextContent.Should().Be("stats.noData");
    }

    [Test]
    public void Products_start_slowest_first_and_sort_by_header_click()
    {
        var cut = Render();

        Column(cut, "products", 0).Should().Equal("Billing", "Web portal", "Mobile");
        Column(cut, "products", 4).Should().Equal("2d 4h", "18h", "—");
        cut.Find("[data-table=products] [data-sort=avg]").ParentElement!.GetAttribute("aria-sort").Should().Be("descending");

        cut.Find("[data-table=products] [data-sort=resolved]").Click();

        Column(cut, "products", 0).Should().Equal("Web portal", "Billing", "Mobile");
        cut.Find("[data-table=products] [data-sort=resolved]").ParentElement!.GetAttribute("aria-sort").Should().Be("descending");
        cut.Find("[data-table=products] [data-sort=avg]").ParentElement!.GetAttribute("aria-sort").Should().Be("none");

        cut.Find("[data-table=products] [data-sort=resolved]").Click();

        Column(cut, "products", 0).Should().Equal("Mobile", "Billing", "Web portal");
        cut.Find("[data-table=products] [data-sort=resolved]").ParentElement!.GetAttribute("aria-sort").Should().Be("ascending");

        cut.Find("[data-table=products] [data-sort=name]").Click();
        Column(cut, "products", 0).Should().Equal("Billing", "Mobile", "Web portal");
    }

    [Test]
    public void Tab_switch_shows_the_team()
    {
        var cut = Render();

        cut.Find("[data-tab=team]").Click();

        cut.Find("[data-tab=team]").GetAttribute("aria-current").Should().Be("true");
        cut.Find("[data-tab=overview]").HasAttribute("aria-current").Should().BeFalse();
        cut.FindAll("[data-trend]").Should().BeEmpty();
        Figure(cut, "with-active").Should().Be("2");
        Figure(cut, "closed").Should().Be("33");
        Figure(cut, "logged").Should().Be("115h");
        cut.Find("[data-workload-note]").TextContent.Should().Be("stats.workloadNote");
    }

    [Test]
    public void Team_rows_show_who_time_average_and_workload_and_mute_idle_rows()
    {
        var cut = Render();
        cut.Find("[data-tab=team]").Click();

        var first = cut.Find("[data-table=team] tbody tr");
        first.TextContent.Should().Contain("Olena Koval").And.Contain("@okoval");
        first.QuerySelector("[data-avatar]")!.TextContent.Should().Be("OK");
        first.Children[4].TextContent.Trim().Should().Be("64h");
        first.Children[5].TextContent.Trim().Should().Be("3h 22m");
        first.Children[6].TextContent.Trim().Should().Be("10");
        first.QuerySelector("[data-mini]")!.GetAttribute("style").Should().Be("width: 100%");

        var idle = cut.Find("[data-table=team] tr[data-idle]");
        idle.TextContent.Should().Contain("Taras Boyko");
        idle.Children[4].TextContent.Trim().Should().Be("—");
        idle.Children[1].ClassList.Should().Contain("text-muted");
    }

    [Test]
    public void Team_starts_with_most_closed_and_sorts_by_header_click()
    {
        var cut = Render();
        cut.Find("[data-tab=team]").Click();

        var closed = cut.Find("[data-table=team] [data-sort=closed]").ParentElement!;
        closed.GetAttribute("aria-sort").Should().Be("descending");
        Column(cut, "team", 3).Should().Equal("19", "14", "0");

        cut.Find("[data-table=team] [data-sort=closed]").Click();

        Column(cut, "team", 3).Should().Equal("0", "14", "19");
        cut.Find("[data-table=team] [data-sort=closed]").ParentElement!.GetAttribute("aria-sort").Should().Be("ascending");

        cut.Find("[data-table=team] [data-sort=name]").Click();

        cut.Find("[data-table=team] [data-sort=name]").ParentElement!.GetAttribute("aria-sort").Should().Be("ascending");
        cut.FindAll("[data-table=team] tbody tr").Select(r => r.GetAttribute("data-row")).Should().Equal("2", "1", "3");
    }

    [Test]
    public void Period_switch_reloads_with_the_new_days_and_the_users_zone()
    {
        var cut = Render();
        _report = Report(days: 7, created: 3);

        Option(cut, "7d").Click();

        _stats.Verify(s => s.GetStatisticsReportAsync(7, _host.TimeZone.Zone), Times.Once);
        Option(cut, "7d").GetAttribute("aria-checked").Should().Be("true");
        Figure(cut, "created").Should().Be("3");
        cut.Find("[data-figure=created] p:last-child").TextContent.Should().Be("# stats.created · 7d");
    }

    [Test]
    public void A_slow_earlier_response_does_not_replace_a_newer_one()
    {
        _report = Report(days: 30);
        var cut = Render();
        var slow = new TaskCompletionSource<StatisticsReportDto>();
        var fast = new TaskCompletionSource<StatisticsReportDto>();
        _stats.Setup(s => s.GetStatisticsReportAsync(7, It.IsAny<TimeZoneInfo>())).Returns(slow.Task);
        _stats.Setup(s => s.GetStatisticsReportAsync(90, It.IsAny<TimeZoneInfo>())).Returns(fast.Task);

        Option(cut, "7d").Click();
        Option(cut, "90d").Click();

        // The previous report stays visible while loading, labelled with its own period; the control shows the request.
        Figure(cut, "created").Should().Be("12");
        cut.Find("[data-figure=created] p:last-child").TextContent.Should().Be("# stats.created · 30d");
        cut.Find("[data-section=trend] h2").TextContent.Should().Contain("· 30d");
        Option(cut, "90d").GetAttribute("aria-checked").Should().Be("true");
        cut.Find("[data-report]").GetAttribute("aria-busy").Should().Be("true");

        cut.InvokeAsync(() => fast.SetResult(Report(days: 90, created: 90))).GetAwaiter().GetResult();
        cut.InvokeAsync(() => slow.SetResult(Report(days: 7, created: 7))).GetAwaiter().GetResult();

        cut.WaitForAssertion(() => Figure(cut, "created").Should().Be("90"));
        cut.Find("[data-figure=created] p:last-child").TextContent.Should().Be("# stats.created · 90d");
        cut.Find("[data-report]").GetAttribute("aria-busy").Should().Be("false");
        cut.FindAll("[data-trend] [data-day]").Should().HaveCount(90);
    }

    [Test]
    public void A_failed_period_switch_keeps_the_shown_period_and_can_be_retried()
    {
        _report = Report(days: 30);
        var cut = Render();
        var calls = 0;
        _stats.Setup(s => s.GetStatisticsReportAsync(7, It.IsAny<TimeZoneInfo>()))
            .Returns(() => ++calls == 1
                ? Task.FromException<StatisticsReportDto>(new InvalidOperationException("timeout"))
                : Task.FromResult(Report(days: 7, created: 7)));

        Option(cut, "7d").Click();

        _host.ToastService.Verify(t => t.ShowToastAsync(It.Is<string>(m => m.Contains("timeout")), ToastType.Error, It.IsAny<int>()), Times.Once);
        Option(cut, "30d").GetAttribute("aria-checked").Should().Be("true");
        Figure(cut, "created").Should().Be("12");
        cut.Find("[data-figure=created] p:last-child").TextContent.Should().Be("# stats.created · 30d");

        Option(cut, "7d").Click();

        calls.Should().Be(2);
        Option(cut, "7d").GetAttribute("aria-checked").Should().Be("true");
        Figure(cut, "created").Should().Be("7");
        cut.Find("[data-figure=created] p:last-child").TextContent.Should().Be("# stats.created · 7d");
    }

    [Test]
    public void Shows_loading_until_the_first_report_arrives()
    {
        var pending = new TaskCompletionSource<StatisticsReportDto>();
        _stats.Setup(s => s.GetStatisticsReportAsync(It.IsAny<int>(), It.IsAny<TimeZoneInfo>())).Returns(pending.Task);

        var cut = Render();

        cut.Find("[data-loading]").TextContent.Should().Be("common.loading");
        cut.FindAll("[data-figures]").Should().BeEmpty();

        cut.InvokeAsync(() => pending.SetResult(Report())).GetAwaiter().GetResult();

        cut.WaitForAssertion(() => cut.FindAll("[data-loading]").Should().BeEmpty());
        cut.FindAll("[data-figure]").Should().NotBeEmpty();
    }

    [Test]
    public void A_failed_load_shows_an_error_toast_and_the_page_stays_usable()
    {
        _stats.Setup(s => s.GetStatisticsReportAsync(30, It.IsAny<TimeZoneInfo>())).ThrowsAsync(new InvalidOperationException("db down"));

        var cut = Render();

        _host.ToastService.Verify(
            t => t.ShowToastAsync(It.Is<string>(m => m.Contains("stats.loadFailed") && m.Contains("db down")), ToastType.Error, It.IsAny<int>()),
            Times.Once);
        cut.Find("[data-load-failed]");

        Option(cut, "7d").Click();

        Figure(cut, "open").Should().Be("42");
    }

    [Test]
    public void The_report_is_reloaded_when_the_browser_reports_its_zone()
    {
        var cut = Render();

        cut.InvokeAsync(() => _host.TimeZone.Set("America/New_York")).GetAwaiter().GetResult();

        cut.WaitForAssertion(() =>
            _stats.Verify(s => s.GetStatisticsReportAsync(30, It.Is<TimeZoneInfo>(z => z == _host.TimeZone.Zone)), Times.Once));
    }

    [TestCase(null, "—")]
    [TestCase(0, "0m")]
    [TestCase(45, "45m")]
    [TestCase(390, "6h 30m")]
    [TestCase(1080, "18h")]
    [TestCase(1800, "1d 6h")]
    [TestCase(2900, "2d")]
    public void Durations_use_at_most_two_units(int? minutes, string expected)
    {
        var cut = Render();

        cut.Instance.Format(minutes is int m ? TimeSpan.FromMinutes(m) : null).Should().Be(expected);
    }

    [Test]
    public void Work_time_is_not_folded_into_days()
    {
        var cut = Render();

        cut.Instance.Format(TimeSpan.FromMinutes(3850), withDays: false).Should().Be("64h 10m");
    }

    /// <summary>
    /// The page with its duration formatting exposed.
    /// </summary>
    public sealed class TestStatisticsPage : StatisticsPage
    {
        public string Format(TimeSpan? duration, bool withDays = true) => this.FormatDuration(duration, withDays);
    }
}
