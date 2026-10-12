using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Common;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Application.Services.Notifications;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Tickets.Models;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Domain.Interfaces.Repositories;
using ServiceDeskSystem.Infrastructure.Data;
using ServiceDeskSystem.Infrastructure.Data.Repository;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class TicketStatisticsTests
{
    // UTC+14: the user's day is ahead of the server's and of UTC most of the time.
    private static readonly TimeZoneInfo Kiritimati = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Kiritimati");

    private Mock<ITicketRepository> _tickets;
    private Mock<ITicketStatusHistoryRepository> _history;
    private Mock<IUserRepository> _users;
    private Mock<IWorkLogRepository> _workLogs;
    private IMemoryCache _memoryCache;
    private TicketService _service;

    private List<Ticket> _ticketData;
    private Dictionary<int, DateTime> _resolvedData;
    private List<User> _userData;
    private List<(int UserId, DateTime DateLogged, int Minutes)> _workLogData;

    [SetUp]
    public void SetUp()
    {
        _ticketData = [];
        _resolvedData = [];
        _userData = [];
        _workLogData = [];

        _tickets = new Mock<ITicketRepository>();
        _history = new Mock<ITicketStatusHistoryRepository>();
        _users = new Mock<IUserRepository>();
        _workLogs = new Mock<IWorkLogRepository>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _tickets.Setup(r => r.GetAllWithProductAndTagsAsync()).ReturnsAsync(() => _ticketData);
        _history.Setup(r => r.GetFirstResolvedTimesAsync()).ReturnsAsync(() => _resolvedData);
        _users.Setup(r => r.GetAllWithPersonAsync()).ReturnsAsync(() => _userData);
        _workLogs.Setup(r => r.GetEntriesSinceAsync(It.IsAny<DateTime>())).ReturnsAsync(() => _workLogData);

        var facade = new Mock<IRepositoryFacade>();
        facade.Setup(f => f.Tickets).Returns(_tickets.Object);
        facade.Setup(f => f.TicketStatusHistory).Returns(_history.Object);
        facade.Setup(f => f.Users).Returns(_users.Object);
        facade.Setup(f => f.WorkLogs).Returns(_workLogs.Object);

        var factory = new Mock<IRepositoryFacadeFactory>();
        factory.Setup(f => f.Create()).Returns(facade.Object);

        _service = new TicketService(
            factory.Object,
            new Mock<INotificationService>().Object,
            new Mock<IRealtimeNotifier>().Object,
            new Mock<IDomainEventDispatcher>().Object,
            _memoryCache,
            new Mock<IAuditService>().Object);
    }

    [TearDown]
    public void TearDown()
    {
        _memoryCache?.Dispose();
    }

    private static DateOnly TodayIn(TimeZoneInfo zone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone));

    /// <summary>UTC instant of a wall-clock time on <paramref name="day"/> in <paramref name="zone"/>.</summary>
    private static DateTime Utc(DateOnly day, int hour, int minute, TimeZoneInfo zone) =>
        TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(new TimeOnly(hour, minute)), zone);

    private static Ticket NewTicket(
        int id,
        DateTime createdAt,
        TicketStatus status = TicketStatus.Open,
        TicketPriority priority = TicketPriority.Medium,
        TicketType type = TicketType.Bug,
        int? developerId = null,
        Product? product = null,
        params Tag[] tags) =>
        new()
        {
            Id = id,
            CreatedAt = createdAt,
            Status = status,
            Priority = priority,
            Type = type,
            DeveloperId = developerId,
            Product = product,
            ProductId = product?.Id,
            Tags = tags.ToList(),
        };

    private static User Developer(int id, string login, string first = "", string last = "", UserRole role = UserRole.Developer, bool active = true) =>
        new() { Id = id, Login = login, Role = role, IsActive = active, Person = new Person { FirstName = first, LastName = last } };

    [TestCase("Pacific/Kiritimati")]
    [TestCase("Pacific/Honolulu")]
    public async Task Period_is_today_and_the_days_before_in_the_users_zone(string zoneId)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
        var today = TodayIn(zone);
        var from = today.AddDays(-6);
        _ticketData =
        [
            NewTicket(1, Utc(from, 0, 30, zone)),
            NewTicket(2, Utc(from.AddDays(-1), 23, 30, zone)),
            NewTicket(3, Utc(today, 0, 5, zone), TicketStatus.Resolved),
            NewTicket(4, Utc(from.AddDays(-3), 9, 0, zone), TicketStatus.Closed),
        ];
        _resolvedData = new()
        {
            [3] = Utc(today, 6, 5, zone),
            [4] = Utc(from.AddDays(-1), 23, 59, zone),
        };

        var report = await _service.GetStatisticsReportAsync(7, zone);

        report.From.Should().Be(from);
        report.To.Should().Be(today);
        report.Period.Created.Should().Be(2);
        report.Period.Resolved.Should().Be(1);
        report.Trend.Select(d => d.Date).Should().Equal(Enumerable.Range(0, 7).Select(i => from.AddDays(i)));
        report.Trend[0].Created.Should().Be(1);
        report.Trend[^1].Should().Be(new DailyFlowDto(today, 1, 1));
    }

    [Test]
    public async Task Work_logs_are_requested_from_a_day_before_the_period_on_the_utc_side()
    {
        var from = TodayIn(Kiritimati).AddDays(-29);

        await _service.GetStatisticsReportAsync(30, Kiritimati);

        _workLogs.Verify(r => r.GetEntriesSinceAsync(from.AddDays(-1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)));
    }

    [Test]
    public async Task Resolution_time_runs_from_creation_to_the_first_done_transition()
    {
        var today = TodayIn(Kiritimati);
        var created = Utc(today.AddDays(-3), 9, 0, Kiritimati);
        _ticketData = [NewTicket(1, created, TicketStatus.Closed), NewTicket(2, created, TicketStatus.Done)];

        // The repository returns the first done change per ticket (reopened and resolved again later).
        _resolvedData = new() { [1] = created.AddHours(30), [2] = created.AddHours(6).AddMinutes(30) };

        var report = await _service.GetStatisticsReportAsync(7, Kiritimati);

        report.Period.Resolved.Should().Be(2);
        report.Period.AverageResolution.Should().Be(TimeSpan.FromHours(18.25));
        report.Period.ResolvedPercent.Should().Be(100);
    }

    [Test]
    public async Task Done_tickets_without_history_and_reopened_tickets_are_not_resolved_in_the_period()
    {
        var today = TodayIn(Kiritimati);
        var created = Utc(today.AddDays(-1), 9, 0, Kiritimati);
        _ticketData =
        [
            NewTicket(1, created, TicketStatus.Closed),
            NewTicket(2, created, TicketStatus.InProgress),
            NewTicket(3, created, TicketStatus.Open),
        ];
        _resolvedData = new() { [2] = created.AddHours(2) };

        var report = await _service.GetStatisticsReportAsync(7, Kiritimati);

        report.Period.Created.Should().Be(3);
        report.Period.Resolved.Should().Be(0);
        report.Period.AverageResolution.Should().BeNull();
        report.Period.ResolvedPercent.Should().Be(0);
        report.Snapshot.ByStatus[TicketStatus.Closed].Should().Be(1);
        report.Snapshot.Open.Should().Be(2);
    }

    [Test]
    public async Task Resolved_percent_is_null_when_nothing_was_created()
    {
        _ticketData = [NewTicket(1, Utc(TodayIn(Kiritimati).AddDays(-100), 9, 0, Kiritimati))];

        var report = await _service.GetStatisticsReportAsync(7, Kiritimati);

        report.Period.Created.Should().Be(0);
        report.Period.ResolvedPercent.Should().BeNull();
    }

    [Test]
    public async Task Snapshot_distributions_count_open_tickets_only()
    {
        var created = Utc(TodayIn(Kiritimati).AddDays(-200), 9, 0, Kiritimati);
        var backend = new Tag { Id = 1, Name = "backend", Color = "#ff0000" };
        var ui = new Tag { Id = 2, Name = "ui" };
        _ticketData =
        [
            NewTicket(1, created, TicketStatus.Open, TicketPriority.Critical, TicketType.Bug, tags: [backend, ui]),
            NewTicket(2, created, TicketStatus.InProgress, TicketPriority.High, TicketType.Support, tags: [backend]),
            NewTicket(3, created, TicketStatus.Closed, TicketPriority.Critical, TicketType.Bug, tags: [backend, ui]),
            NewTicket(4, created, TicketStatus.Done, TicketPriority.Low, TicketType.Project),
        ];

        var report = await _service.GetStatisticsReportAsync(30, Kiritimati);

        var snapshot = report.Snapshot;
        snapshot.Open.Should().Be(2);
        snapshot.CriticalOpen.Should().Be(1);
        snapshot.ByStatus.Should().HaveCount(Enum.GetValues<TicketStatus>().Length);
        snapshot.ByStatus[TicketStatus.Closed].Should().Be(1);
        snapshot.ByStatus[TicketStatus.Done].Should().Be(1);
        snapshot.ByPriority[TicketPriority.Critical].Should().Be(1);
        snapshot.ByPriority[TicketPriority.Low].Should().Be(0);
        snapshot.ByType[TicketType.Bug].Should().Be(1);
        snapshot.ByType[TicketType.Project].Should().Be(0);
        snapshot.Tags.Should().Equal(new TagCountDto(1, "backend", 2), new TagCountDto(2, "ui", 1));
    }

    [Test]
    public async Task Products_show_open_created_resolved_and_hide_empty_ones()
    {
        var today = TodayIn(Kiritimati);
        var billing = new Product { Id = 1, Name = "Billing" };
        var legacy = new Product { Id = 2, Name = "Legacy" };
        var recent = Utc(today.AddDays(-2), 9, 0, Kiritimati);
        _ticketData =
        [
            NewTicket(1, recent, TicketStatus.Resolved, product: billing),
            NewTicket(2, recent, TicketStatus.Open, product: billing),
            NewTicket(3, Utc(today.AddDays(-300), 9, 0, Kiritimati), TicketStatus.Closed, product: legacy),
        ];
        _resolvedData = new() { [1] = recent.AddHours(4) };

        var report = await _service.GetStatisticsReportAsync(7, Kiritimati);

        report.Products.Should().ContainSingle()
            .Which.Should().Be(new ProductStatisticsDto(1, "Billing", 1, 2, 1, TimeSpan.FromHours(4)));
    }

    [Test]
    public async Task Team_rows_follow_the_workload_formula()
    {
        var today = TodayIn(Kiritimati);
        var created = Utc(today.AddDays(-2), 9, 0, Kiritimati);
        _userData = [Developer(7, "okoval", "Olena", "Koval")];
        _ticketData =
        [
            NewTicket(1, created, TicketStatus.InProgress, developerId: 7),
            NewTicket(2, created, TicketStatus.CodeReview, developerId: 7),
            NewTicket(3, created, TicketStatus.Open, developerId: 7),
            NewTicket(4, created, TicketStatus.Closed, developerId: 7),
            NewTicket(5, created, TicketStatus.Testing),
        ];
        _resolvedData = new() { [4] = created.AddHours(1) };

        var report = await _service.GetStatisticsReportAsync(7, Kiritimati);

        var row = report.Team.Should().ContainSingle().Subject;
        row.DisplayName.Should().Be("Olena Koval");
        row.Login.Should().Be("okoval");
        row.InProgress.Should().Be(2);
        row.Active.Should().Be(3);
        row.ClosedInPeriod.Should().Be(1);
        row.Workload.Should().Be(5);
    }

    [Test]
    public async Task Work_log_time_counts_only_days_inside_the_period()
    {
        var today = TodayIn(Kiritimati);
        var from = today.AddDays(-6);
        var created = Utc(today.AddDays(-1), 9, 0, Kiritimati);
        _userData = [Developer(7, "okoval")];
        _ticketData = [NewTicket(1, created, TicketStatus.Done, developerId: 7), NewTicket(2, created, TicketStatus.Done, developerId: 7)];
        _resolvedData = new() { [1] = created.AddHours(1), [2] = created.AddHours(2) };

        // Work logs store the UTC instant of the picked local midnight.
        _workLogData =
        [
            (7, Utc(from, 0, 0, Kiritimati), 90),
            (7, Utc(today, 0, 0, Kiritimati), 45),
            (7, Utc(from.AddDays(-1), 0, 0, Kiritimati), 600),
            (8, Utc(today, 0, 0, Kiritimati), 30),
        ];

        var report = await _service.GetStatisticsReportAsync(7, Kiritimati);

        var row = report.Team.Should().ContainSingle().Subject;
        row.MinutesInPeriod.Should().Be(135);
        row.AverageMinutesPerTicket.Should().Be(67.5);
    }

    [Test]
    public async Task Active_developers_are_listed_without_tickets_and_admins_only_with_activity()
    {
        var created = Utc(TodayIn(Kiritimati).AddDays(-1), 9, 0, Kiritimati);
        _userData =
        [
            Developer(1, "idle-dev"),
            Developer(2, "gone-dev", active: false),
            Developer(3, "admin", role: UserRole.Admin),
            Developer(4, "busy-admin", "Taras", role: UserRole.Admin),
            Developer(5, "client", role: UserRole.User),
        ];
        _ticketData = [NewTicket(1, created, TicketStatus.Open, developerId: 4), NewTicket(2, created, TicketStatus.Open, developerId: 5)];

        var report = await _service.GetStatisticsReportAsync(30, Kiritimati);

        report.Team.Select(r => r.Login).Should().Equal("busy-admin", "idle-dev");
        report.Team[0].DisplayName.Should().Be("Taras");
        report.Team[1].DisplayName.Should().Be("idle-dev");
        report.Team[1].IsIdle.Should().BeTrue();
        report.Team[1].AverageMinutesPerTicket.Should().BeNull();
    }

    [Test]
    public async Task Team_is_sorted_by_closed_then_workload()
    {
        var created = Utc(TodayIn(Kiritimati).AddDays(-1), 9, 0, Kiritimati);
        _userData = [Developer(1, "a"), Developer(2, "b"), Developer(3, "c")];
        _ticketData =
        [
            NewTicket(1, created, TicketStatus.Open, developerId: 1),
            NewTicket(2, created, TicketStatus.InProgress, developerId: 2),
            NewTicket(3, created, TicketStatus.Resolved, developerId: 3),
        ];
        _resolvedData = new() { [3] = created.AddHours(1) };

        var report = await _service.GetStatisticsReportAsync(7, Kiritimati);

        report.Team.Select(r => r.Login).Should().Equal("c", "b", "a");
    }

    [Test]
    public async Task Repository_returns_the_first_done_change_per_ticket()
    {
        var options = new DbContextOptionsBuilder<BugTrackerDbContext>()
            .UseInMemoryDatabase($"FirstResolved_{Guid.NewGuid()}")
            .Options;
        var day = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc);

        await using (var seed = new BugTrackerDbContext(options))
        {
            seed.TicketStatusHistory.AddRange(
                new TicketStatusHistory { TicketId = 1, OldStatus = TicketStatus.Open, NewStatus = TicketStatus.InProgress, ChangedAt = day },
                new TicketStatusHistory { TicketId = 1, OldStatus = TicketStatus.InProgress, NewStatus = TicketStatus.Resolved, ChangedAt = day.AddDays(1) },
                new TicketStatusHistory { TicketId = 1, OldStatus = TicketStatus.Resolved, NewStatus = TicketStatus.InProgress, ChangedAt = day.AddDays(2) },
                new TicketStatusHistory { TicketId = 1, OldStatus = TicketStatus.InProgress, NewStatus = TicketStatus.Closed, ChangedAt = day.AddDays(3) },
                new TicketStatusHistory { TicketId = 2, OldStatus = TicketStatus.Testing, NewStatus = TicketStatus.Done, ChangedAt = day.AddDays(4) },
                new TicketStatusHistory { TicketId = 3, OldStatus = TicketStatus.Open, NewStatus = TicketStatus.CodeReview, ChangedAt = day });
            await seed.SaveChangesAsync();
        }

        await using var context = new BugTrackerDbContext(options);
        var times = await new TicketStatusHistoryRepository(context).GetFirstResolvedTimesAsync();

        times.Should().BeEquivalentTo(new Dictionary<int, DateTime> { [1] = day.AddDays(1), [2] = day.AddDays(4) });
    }

    [Test]
    public async Task Repository_returns_work_logs_of_all_users_since_the_date()
    {
        var options = new DbContextOptionsBuilder<BugTrackerDbContext>()
            .UseInMemoryDatabase($"WorkLogsSince_{Guid.NewGuid()}")
            .Options;
        var since = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

        await using (var seed = new BugTrackerDbContext(options))
        {
            seed.WorkLogs.AddRange(
                new WorkLog { UserId = 1, TicketId = 1, TimeSpentMinutes = 30, DateLogged = since },
                new WorkLog { UserId = 2, TicketId = 1, TimeSpentMinutes = 45, DateLogged = since.AddDays(3) },
                new WorkLog { UserId = 1, TicketId = 1, TimeSpentMinutes = 60, DateLogged = since.AddMinutes(-1) });
            await seed.SaveChangesAsync();
        }

        await using var context = new BugTrackerDbContext(options);
        var entries = await new WorkLogRepository(context).GetEntriesSinceAsync(since);

        entries.Should().BeEquivalentTo([(1, since, 30), (2, since.AddDays(3), 45)]);
    }
}
