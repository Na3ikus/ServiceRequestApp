using FluentAssertions;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Profile;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Domain.Interfaces.Repositories;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class UserActivityServiceTests
{
    private const int UserId = 7;

    private Mock<ITicketRepository> _tickets;
    private Mock<ICommentRepository> _comments;
    private Mock<ITicketStatusHistoryRepository> _history;
    private Mock<IWorkLogRepository> _workLogs;
    private UserActivityService _service;

    [SetUp]
    public void SetUp()
    {
        _tickets = new Mock<ITicketRepository>();
        _comments = new Mock<ICommentRepository>();
        _history = new Mock<ITicketStatusHistoryRepository>();
        _workLogs = new Mock<IWorkLogRepository>();

        _tickets.Setup(r => r.GetCreatedTimesByAuthorAsync(UserId, It.IsAny<DateTime>())).ReturnsAsync([]);
        _comments.Setup(r => r.GetCreatedTimesByAuthorAsync(UserId, It.IsAny<DateTime>())).ReturnsAsync([]);
        _history.Setup(r => r.GetChangeTimesByUserAsync(UserId, It.IsAny<DateTime>())).ReturnsAsync([]);
        _workLogs.Setup(r => r.GetEntriesByUserSinceAsync(UserId, It.IsAny<DateTime>())).ReturnsAsync([]);

        var facade = new Mock<IRepositoryFacade>();
        facade.Setup(f => f.Tickets).Returns(_tickets.Object);
        facade.Setup(f => f.Comments).Returns(_comments.Object);
        facade.Setup(f => f.TicketStatusHistory).Returns(_history.Object);
        facade.Setup(f => f.WorkLogs).Returns(_workLogs.Object);

        var factory = new Mock<IRepositoryFacadeFactory>();
        factory.Setup(f => f.Create()).Returns(facade.Object);

        _service = new UserActivityService(factory.Object);
    }

    // Noon UTC stays on the same calendar day in every time zone between UTC-11 and UTC+11.
    private static DateTime NoonUtc(int month, int day) => new(2026, month, day, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task Merges_all_four_sources_into_one_day()
    {
        _tickets.Setup(r => r.GetCreatedTimesByAuthorAsync(UserId, It.IsAny<DateTime>())).ReturnsAsync([NoonUtc(10, 5)]);
        _comments.Setup(r => r.GetCreatedTimesByAuthorAsync(UserId, It.IsAny<DateTime>())).ReturnsAsync([NoonUtc(10, 5), NoonUtc(10, 5)]);
        _history.Setup(r => r.GetChangeTimesByUserAsync(UserId, It.IsAny<DateTime>())).ReturnsAsync([NoonUtc(10, 5)]);
        _workLogs.Setup(r => r.GetEntriesByUserSinceAsync(UserId, It.IsAny<DateTime>()))
            .ReturnsAsync([(new DateTime(2026, 10, 5), 90)]);

        var result = await _service.GetDailyActivityAsync(UserId, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 10));

        result.Should().ContainSingle();
        var day = result[0];
        day.Date.Should().Be(new DateOnly(2026, 10, 5));
        day.TicketsCreated.Should().Be(1);
        day.Comments.Should().Be(2);
        day.StatusChanges.Should().Be(1);
        day.WorkLogs.Should().Be(1);
        day.WorkMinutes.Should().Be(90);
        day.Total.Should().Be(5);
    }

    [Test]
    public async Task Returns_only_active_days_in_range_ordered_by_date()
    {
        _comments.Setup(r => r.GetCreatedTimesByAuthorAsync(UserId, It.IsAny<DateTime>()))
            .ReturnsAsync([NoonUtc(10, 8), NoonUtc(9, 1), NoonUtc(10, 2), NoonUtc(10, 20)]);

        var result = await _service.GetDailyActivityAsync(UserId, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 10));

        result.Select(d => d.Date).Should().Equal(new DateOnly(2026, 10, 2), new DateOnly(2026, 10, 8));
    }

    [Test]
    public async Task Work_log_date_is_used_as_calendar_day()
    {
        _workLogs.Setup(r => r.GetEntriesByUserSinceAsync(UserId, It.IsAny<DateTime>()))
            .ReturnsAsync([(new DateTime(2026, 10, 3), 30), (new DateTime(2026, 10, 3), 15)]);

        var result = await _service.GetDailyActivityAsync(UserId, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 10));

        result.Should().ContainSingle().Which.Should().Match<ServiceDeskSystem.Application.Services.Profile.Models.DailyActivityDto>(
            d => d.Date == new DateOnly(2026, 10, 3) && d.WorkLogs == 2 && d.WorkMinutes == 45);
    }

    [Test]
    public async Task Queries_sources_from_one_day_before_range_start()
    {
        await _service.GetDailyActivityAsync(UserId, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 10));

        var expected = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc);
        _tickets.Verify(r => r.GetCreatedTimesByAuthorAsync(UserId, expected));
        _history.Verify(r => r.GetChangeTimesByUserAsync(UserId, expected));
    }

    [Test]
    public async Task Inverted_range_returns_empty_without_queries()
    {
        var result = await _service.GetDailyActivityAsync(UserId, new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 1));

        result.Should().BeEmpty();
        _tickets.VerifyNoOtherCalls();
    }
}
