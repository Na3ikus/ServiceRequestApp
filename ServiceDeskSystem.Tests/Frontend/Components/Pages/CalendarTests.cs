using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Components.Pages.Calendar;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Pages;

[TestFixture]
public class CalendarTests
{
    private static readonly DateTime NowUtc = new(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc);

    private ComponentTestHost _host;
    private Mock<ITicketService> _tickets;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();
        _host.TimeZone.Set("UTC");
        _tickets = new Mock<ITicketService>();
        _tickets.Setup(t => t.GetAllTicketsAsync()).ReturnsAsync([]);
        _host.Context.Services.AddSingleton(_tickets.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private static Ticket Ticket(int id, DateTime created, DateTime? due = null, TicketStatus status = TicketStatus.Open) =>
        new() { Id = id, Title = $"T{id}", CreatedAt = created, DueDate = due, Status = status };

    private static DateTime Utc(int month, int day, int hour = 12) => new(2026, month, day, hour, 0, 0, DateTimeKind.Utc);

    private static IReadOnlyDictionary<DateOnly, IReadOnlyList<CalendarEvent>> October(params Ticket[] tickets) =>
        CalendarEvents.ForMonth(tickets, 2026, 10, utc => utc, NowUtc);

    [Test]
    public void Start_and_due_land_on_their_days()
    {
        var events = October(Ticket(1, Utc(10, 2), Utc(10, 20)));

        events[new DateOnly(2026, 10, 2)].Should().ContainSingle().Which.Kind.Should().Be(CalendarEventKind.Start);
        events[new DateOnly(2026, 10, 20)].Should().ContainSingle().Which.Kind.Should().Be(CalendarEventKind.Due);
    }

    [Test]
    public void Missed_deadline_is_overdue_only_while_the_ticket_is_open()
    {
        var open = October(Ticket(1, Utc(10, 1), Utc(10, 5)));
        var closed = October(Ticket(2, Utc(10, 1), Utc(10, 5), TicketStatus.Closed));

        open[new DateOnly(2026, 10, 5)][0].Kind.Should().Be(CalendarEventKind.Overdue);
        closed[new DateOnly(2026, 10, 5)][0].Kind.Should().Be(CalendarEventKind.Due);
    }

    [Test]
    public void Same_day_start_and_deadline_show_once_and_deadlines_come_first()
    {
        var events = October(Ticket(5, Utc(10, 9), Utc(10, 9, 18)), Ticket(1, Utc(10, 9)));

        events[new DateOnly(2026, 10, 9)].Select(e => (e.Ticket.Id, e.Kind))
            .Should().Equal((5, CalendarEventKind.Overdue), (1, CalendarEventKind.Start));
    }

    [Test]
    public void Days_follow_the_users_zone_and_other_months_are_dropped()
    {
        // 23:30 UTC on 31 Oct is already 1 Nov in Kyiv.
        var kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        var events = CalendarEvents.ForMonth(
            [Ticket(1, new DateTime(2026, 10, 31, 23, 30, 0, DateTimeKind.Utc))], 2026, 10,
            utc => TimeZoneInfo.ConvertTimeFromUtc(utc, kyiv), NowUtc);

        events.Should().BeEmpty();
    }

    [Test]
    public void Page_has_path_header_culture_day_names_and_no_hero()
    {
        var cut = _host.Context.RenderComponent<CalendarPage>();

        cut.Find("h1").TextContent.Should().Be("~/calendar");
        cut.FindAll("[data-day-name]").Select(d => d.TextContent).Should().HaveCount(7).And.StartWith("mon");
        cut.Markup.Should().NotContain("page-hero").And.NotContain("gradient").And.NotContain("⏰");
        cut.FindAll("[data-today-mark]").Should().ContainSingle();
    }

    [Test]
    public void Busy_day_shows_three_chips_and_a_more_button_that_opens_the_day()
    {
        var today = _host.TimeZone.Today;
        var at = new DateTime(today.Year, today.Month, today.Day, 10, 0, 0, DateTimeKind.Utc);
        _tickets.Setup(t => t.GetAllTicketsAsync()).ReturnsAsync(Enumerable.Range(1, 5).Select(i => Ticket(i, at)).ToList());

        var cut = _host.Context.RenderComponent<CalendarPage>();
        var cell = cut.Find($"[data-day='{today:yyyy-MM-dd}']");

        cell.QuerySelectorAll("[data-chip]").Length.Should().Be(3);
        cut.Find("[data-more]").TextContent.Trim().Should().Be("+2");

        cut.Find("[data-more]").Click();

        cut.Find("[data-day-panel]").QuerySelectorAll("li").Length.Should().Be(5);
    }

    [Test]
    public void Navigation_changes_the_month_and_today_returns()
    {
        var cut = _host.Context.RenderComponent<CalendarPage>();
        var title = cut.Find("[data-month-title]").TextContent;

        cut.Find("button[aria-label='calendar.nextMonth']").Click();
        cut.Find("[data-month-title]").TextContent.Should().NotBe(title);
        cut.FindAll("[data-today-mark]").Should().BeEmpty();

        cut.Find("[data-today]").Click();
        cut.Find("[data-month-title]").TextContent.Should().Be(title);
    }
}
