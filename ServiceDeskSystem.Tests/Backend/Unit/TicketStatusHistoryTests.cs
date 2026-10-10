using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Common;
using ServiceDeskSystem.Application.EventHandlers;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Events;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Infrastructure.Data;
using ServiceDeskSystem.Infrastructure.Data.Repository;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class TicketStatusHistoryTests
{
    private static DomainEventNotification<TicketStatusChangedEvent> Notification(int? actor = 3) =>
        new(new TicketStatusChangedEvent(42, TicketStatus.Open, TicketStatus.InProgress, actor));

    [Test]
    public async Task Handler_writes_a_history_row()
    {
        var history = new Mock<ITicketStatusHistoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var facade = new Mock<IRepositoryFacade>();
        facade.Setup(f => f.TicketStatusHistory).Returns(history.Object);
        facade.Setup(f => f.UnitOfWork).Returns(unitOfWork.Object);
        var factory = new Mock<IRepositoryFacadeFactory>();
        factory.Setup(f => f.Create()).Returns(facade.Object);

        TicketStatusHistory? saved = null;
        history.Setup(h => h.CreateAsync(It.IsAny<TicketStatusHistory>())).Callback<TicketStatusHistory>(e => saved = e);

        var handler = new TicketStatusHistoryEventHandler(factory.Object, NullLogger<TicketStatusHistoryEventHandler>.Instance);
        await handler.Handle(Notification(), CancellationToken.None);

        saved.Should().NotBeNull();
        saved!.TicketId.Should().Be(42);
        saved.OldStatus.Should().Be(TicketStatus.Open);
        saved.NewStatus.Should().Be(TicketStatus.InProgress);
        saved.ChangedByUserId.Should().Be(3);
        saved.ChangedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task Handler_swallows_repository_errors()
    {
        var factory = new Mock<IRepositoryFacadeFactory>();
        factory.Setup(f => f.Create()).Throws(new InvalidOperationException("db down"));

        var handler = new TicketStatusHistoryEventHandler(factory.Object, NullLogger<TicketStatusHistoryEventHandler>.Instance);

        var act = () => handler.Handle(Notification(), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task Repository_returns_only_the_users_changes_since_date()
    {
        var options = new DbContextOptionsBuilder<BugTrackerDbContext>()
            .UseInMemoryDatabase($"StatusHistory_{Guid.NewGuid()}")
            .Options;

        await using (var seed = new BugTrackerDbContext(options))
        {
            seed.TicketStatusHistory.AddRange(
                new TicketStatusHistory { TicketId = 1, OldStatus = TicketStatus.Open, NewStatus = TicketStatus.CodeReview, ChangedByUserId = 3, ChangedAt = new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc) },
                new TicketStatusHistory { TicketId = 1, OldStatus = TicketStatus.CodeReview, NewStatus = TicketStatus.Done, ChangedByUserId = 3, ChangedAt = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc) },
                new TicketStatusHistory { TicketId = 2, OldStatus = TicketStatus.New, NewStatus = TicketStatus.Open, ChangedByUserId = 4, ChangedAt = new DateTime(2026, 10, 6, 0, 0, 0, DateTimeKind.Utc) });
            await seed.SaveChangesAsync();
        }

        await using var context = new BugTrackerDbContext(options);
        var repo = new TicketStatusHistoryRepository(context);

        var times = await repo.GetChangeTimesByUserAsync(3, new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));

        times.Should().Equal(new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc));
        context.TicketStatusHistory.Single(h => h.ChangedByUserId == 3 && h.NewStatus == TicketStatus.CodeReview)
            .OldStatus.Should().Be(TicketStatus.Open);
    }
}
