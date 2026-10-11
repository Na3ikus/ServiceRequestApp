using Bunit;
using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Profile.Models;
using ServiceDeskSystem.Components.UI.Elements;

namespace ServiceDeskSystem.Tests.Frontend.Components.Pages;

[TestFixture]
public class ActivityHeatmapTests
{
    // A Wednesday: the last column holds Mon 12 Oct – Wed 14 Oct.
    private static readonly DateOnly Today = new(2026, 10, 14);

    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    private IRenderedComponent<ActivityHeatmap> Render(params DailyActivityDto[] days) =>
        _host.Context.RenderComponent<ActivityHeatmap>(p => p
            .Add(x => x.Days, days)
            .Add(x => x.Today, Today));

    private static DailyActivityDto Day(DateOnly date, int tickets, int comments = 0, int status = 0, int logs = 0, int minutes = 0) =>
        new(date, tickets, comments, status, logs, minutes);

    [Test]
    public void First_monday_starts_a_26_week_grid_ending_this_week()
    {
        ActivityHeatmap.FirstMonday(Today).Should().Be(new DateOnly(2026, 4, 20));
        ActivityHeatmap.FirstMonday(new DateOnly(2026, 10, 12)).Should().Be(new DateOnly(2026, 4, 20));
        ActivityHeatmap.FirstMonday(new DateOnly(2026, 10, 18)).Should().Be(new DateOnly(2026, 4, 20));
    }

    [TestCase(0, 8, 0)]
    [TestCase(1, 8, 1)]
    [TestCase(2, 8, 1)]
    [TestCase(3, 8, 2)]
    [TestCase(6, 8, 3)]
    [TestCase(8, 8, 4)]
    public void Levels_are_relative_to_the_busiest_day(int total, int busiest, int expected) =>
        ActivityHeatmap.Level(total, busiest).Should().Be(expected);

    [Test]
    public void Renders_one_cell_per_day_up_to_today()
    {
        var cut = Render();

        // 25 full weeks + Mon..Wed of the current one.
        cut.FindAll("[data-cell]").Should().HaveCount((25 * 7) + 3);
    }

    [Test]
    public void Empty_map_has_only_level_zero_and_a_zero_summary()
    {
        var cut = Render();

        cut.FindAll("[data-cell]").Select(c => c.GetAttribute("data-level")).Should().OnlyContain(l => l == "0");
        cut.Find("[data-summary]").TextContent.Should().Be("profile.activitySummary");
        cut.Find("[data-heatmap]").GetAttribute("role").Should().Be("img");
    }

    [Test]
    public void Busiest_day_is_level_four_with_breakdown_tooltip()
    {
        _host.Localization.Setup(l => l.Translate("profile.activityTooltip"))
            .Returns("{0} · {1} actions — tickets {2}, comments {3}, status {4}, time {5}");
        _host.Localization.Setup(l => l.Translate("profile.activitySummary")).Returns("{0} actions in 6 months");

        var cut = Render(Day(Today.AddDays(-2), 2, 3, 1, 1, 90), Day(Today, 1));

        var cells = cut.FindAll("[data-cell]").ToList();
        var busy = cells[^3];
        busy.GetAttribute("data-level").Should().Be("4");
        busy.GetAttribute("title").Should().Be("12 Oct · 7 actions — tickets 2, comments 3, status 1, time 1h 30m");
        cells[^1].GetAttribute("data-level").Should().Be("1");
        cut.Find("[data-heatmap]").GetAttribute("aria-label").Should().Be("8 actions in 6 months");
        cells.Should().AllSatisfy(c => c.GetAttribute("aria-hidden").Should().Be("true"));
    }

    [Test]
    public void Skeleton_while_loading()
    {
        var cut = _host.Context.RenderComponent<ActivityHeatmap>(p => p.Add(x => x.Days, null));

        cut.FindAll("[data-cell]").Should().BeEmpty();
        cut.Find("[aria-busy=true]").Should().NotBeNull();
    }
}
