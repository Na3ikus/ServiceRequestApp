using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Common;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Application.Services.Notifications;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Domain.Common;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Events;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class TicketServiceTests
{
    private Mock<IRepositoryFacadeFactory> _mockFactory;
    private Mock<IRepositoryFacade> _mockFacade;
    private Mock<ITicketRepository> _mockTicketRepo;
    private Mock<ISlaPolicyRepository> _mockSlaPolicyRepo;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<INotificationService> _mockNotificationService;
    private Mock<IRealtimeNotifier> _mockRealtimeNotifier;
    private Mock<IDomainEventDispatcher> _mockDispatcher;
    private IMemoryCache _memoryCache;
    private Mock<IAuditService> _mockAuditService;
    private TicketService _ticketService;

    [SetUp]
    public void SetUp()
    {
        _mockFactory = new Mock<IRepositoryFacadeFactory>();
        _mockFacade = new Mock<IRepositoryFacade>();
        _mockTicketRepo = new Mock<ITicketRepository>();
        _mockSlaPolicyRepo = new Mock<ISlaPolicyRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockRealtimeNotifier = new Mock<IRealtimeNotifier>();
        _mockDispatcher = new Mock<IDomainEventDispatcher>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockAuditService = new Mock<IAuditService>();

        _mockFactory.Setup(f => f.Create()).Returns(_mockFacade.Object);
        _mockFacade.Setup(f => f.Tickets).Returns(_mockTicketRepo.Object);
        _mockFacade.Setup(f => f.SlaPolicies).Returns(_mockSlaPolicyRepo.Object);
        _mockFacade.Setup(f => f.UnitOfWork).Returns(_mockUnitOfWork.Object);
        _mockSlaPolicyRepo.Setup(r => r.GetActiveForPriorityAsync(It.IsAny<TicketPriority>()))
            .ReturnsAsync(new List<SlaPolicy>());

        _ticketService = new TicketService(
            _mockFactory.Object,
            _mockNotificationService.Object,
            _mockRealtimeNotifier.Object,
            _mockDispatcher.Object,
            _memoryCache,
            _mockAuditService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _memoryCache.Dispose();
    }

    [Test]
    public async Task CreateTicketAsync_NonProjectWithoutProduct_ThrowsArgumentException()
    {
        // Arrange
        var ticket = new Ticket
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Type = TicketType.Bug,
            Priority = TicketPriority.Medium,
            AuthorId = 1,
            ProductId = null // Product ID is missing!
        };

        // Act & Assert
        Func<Task> act = async () => await _ticketService.CreateTicketAsync(ticket);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Product is required for non-project tickets. (Parameter 'ticket')");
    }

    [Test]
    public async Task CreateTicketAsync_ProjectWithoutProduct_Succeeds()
    {
        // Arrange
        var ticket = new Ticket
        {
            Title = "Test Project Ticket",
            Description = "Test Description",
            Type = TicketType.Project,
            Priority = TicketPriority.Medium,
            AuthorId = 1,
            ProductId = null // Project doesn't require a product
        };

        _mockTicketRepo.Setup(r => r.CreateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockDispatcher.Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _ticketService.CreateTicketAsync(ticket);

        // Assert
        result.Should().NotBeNull();
        _mockTicketRepo.Verify(r => r.CreateAsync(ticket), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [TestCase(TicketPriority.Critical, 4, true)]  // Critical priority is in hours
    [TestCase(TicketPriority.High, 2, false)]     // High priority is in days
    [TestCase(TicketPriority.Medium, 5, false)]   // Medium priority is in days
    [TestCase(TicketPriority.Low, 14, false)]     // Low priority is in days
    public async Task CreateTicketAsync_ValidTicket_AutoCalculatesSlaDueDate(TicketPriority priority, int expectedDuration, bool inHours)
    {
        // Arrange
        var ticket = new Ticket
        {
            Title = "Test SLA Due Date Calculation",
            Description = "Test Description",
            Type = TicketType.Project,
            Priority = priority,
            AuthorId = 1,
            ProductId = null
        };

        _mockTicketRepo.Setup(r => r.CreateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockDispatcher.Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _ticketService.CreateTicketAsync(ticket);

        // Assert
        result.DueDate.Should().NotBeNull();

        if (inHours)
        {
            var timeDifference = result.DueDate!.Value - result.CreatedAt;
            timeDifference.TotalHours.Should().BeApproximately(expectedDuration, 0.1);
        }
        else
        {
            result.DueDate!.Value.Date.Should().Be(result.CreatedAt.AddDays(expectedDuration).Date);
            result.DueDate!.Value.Hour.Should().Be(16);
            result.DueDate!.Value.Minute.Should().Be(0);
            result.DueDate!.Value.Second.Should().Be(0);
        }
    }

    [Test]
    public async Task CreateTicketAsync_WithMatchingSlaPolicy_UsesPolicyDeadlines()
    {
        var policy = new SlaPolicy
        {
            Id = 42,
            Priority = TicketPriority.High,
            ProductId = null,
            IsActive = true,
            ResponseMinutes = 90,
            ResolutionMinutes = 600,
        };

        _mockSlaPolicyRepo.Setup(r => r.GetActiveForPriorityAsync(TicketPriority.High))
            .ReturnsAsync(new List<SlaPolicy> { policy });
        _mockTicketRepo.Setup(r => r.CreateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockDispatcher.Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var ticket = new Ticket
        {
            Title = "Policy driven deadlines",
            Description = "Test Description",
            Type = TicketType.Project,
            Priority = TicketPriority.High,
            AuthorId = 1,
            ProductId = null,
        };

        var result = await _ticketService.CreateTicketAsync(ticket);

        result.SlaPolicyId.Should().Be(42);
        result.ResponseDueAt.Should().Be(result.CreatedAt.AddMinutes(90));
        result.DueDate.Should().Be(result.CreatedAt.AddMinutes(600));
    }

    [Test]
    public async Task AssignDeveloperAsync_RecordsFirstResponseTime()
    {
        var ticket = new Ticket { Id = 11, Title = "Unassigned", AuthorId = 1 };

        _mockTicketRepo.Setup(r => r.GetByIdAsync(11)).ReturnsAsync(ticket);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockRealtimeNotifier.Setup(n => n.NotifyTicketsChangedAsync()).Returns(Task.CompletedTask);

        await _ticketService.AssignDeveloperAsync(11, developerId: 3);

        ticket.FirstRespondedAt.Should().NotBeNull();

        var firstResponse = ticket.FirstRespondedAt;
        await _ticketService.AssignDeveloperAsync(11, developerId: 4);

        ticket.FirstRespondedAt.Should().Be(firstResponse);
    }

    [Test]
    public async Task Edit_based_on_the_current_version_is_saved_and_moves_the_version_on()
    {
        var ticket = new Ticket { Id = 12, Title = "Fresh", AuthorId = 1, Version = 4 };
        _mockTicketRepo.Setup(r => r.GetByIdAsync(12)).ReturnsAsync(ticket);

        var result = await _ticketService.AssignDeveloperAsync(12, developerId: 3, expectedVersion: 4);

        result.Should().Be(TicketUpdateResult.Updated);
        ticket.DeveloperId.Should().Be(3);
        ticket.Version.Should().Be(5);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task Edit_based_on_an_older_version_is_a_conflict_and_saves_nothing()
    {
        // Someone else took the ticket after this user opened it.
        var ticket = new Ticket { Id = 13, Title = "Taken", AuthorId = 1, DeveloperId = 8, Version = 2 };
        _mockTicketRepo.Setup(r => r.GetByIdAsync(13)).ReturnsAsync(ticket);

        var result = await _ticketService.AssignDeveloperAsync(13, developerId: 3, expectedVersion: 1);

        result.Should().Be(TicketUpdateResult.Conflict);
        ticket.DeveloperId.Should().Be(8);
        ticket.Version.Should().Be(2);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        _mockRealtimeNotifier.Verify(n => n.NotifyTicketsChangedAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Edit_of_a_missing_ticket_is_not_found()
    {
        _mockTicketRepo.Setup(r => r.GetByIdAsync(404)).ReturnsAsync((Ticket?)null);

        var result = await _ticketService.UpdateTicketPriorityAsync(404, TicketPriority.High, expectedVersion: 0);

        result.Should().Be(TicketUpdateResult.NotFound);
    }

    [Test]
    public async Task Two_simultaneous_edits_of_the_same_version_let_exactly_one_through()
    {
        // Both developers press "take" on version 0 at once. Like a database, every load hands out its own
        // copy of what is stored, and a slow load lets the second read version 0 before the first saves,
        // unless something keeps the two edits apart.
        var storedVersion = 0;
        var loaded = new List<Ticket>();
        _mockTicketRepo.Setup(r => r.GetByIdAsync(14)).Returns(async () =>
        {
            var copy = new Ticket { Id = 14, Title = "Race", AuthorId = 1, Version = storedVersion };
            lock (loaded)
            {
                loaded.Add(copy);
            }

            await Task.Delay(50);
            return copy;
        });
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(() =>
        {
            lock (loaded)
            {
                storedVersion = loaded.Max(t => t.Version);
            }

            return 1;
        });

        var results = await Task.WhenAll(
            Task.Run(() => _ticketService.AssignDeveloperAsync(14, developerId: 3, expectedVersion: 0)),
            Task.Run(() => _ticketService.AssignDeveloperAsync(14, developerId: 4, expectedVersion: 0)));

        results.Should().BeEquivalentTo([TicketUpdateResult.Updated, TicketUpdateResult.Conflict]);
        storedVersion.Should().Be(1);
    }

    [Test]
    public async Task CreateTicketAsync_UsesTicketCreateFactory_PreventsDuplicateCreatedEvents()
    {
        // Arrange
        // Create ticket using factory method (which registers a TicketCreatedEvent)
        var ticket = Ticket.Create(
            "Factory created ticket",
            "Description",
            TicketType.Project,
            TicketPriority.Medium,
            1,
            null);

        ticket.DomainEvents.Should().ContainSingle(e => e is TicketCreatedEvent);

        _mockTicketRepo.Setup(r => r.CreateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockDispatcher.Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _ticketService.CreateTicketAsync(ticket);

        // Verify dispatcher only receives one TicketCreatedEvent
        _mockDispatcher.Verify(d => d.DispatchAsync(It.Is<IEnumerable<IDomainEvent>>(events =>
            events.Count(e => e is TicketCreatedEvent) == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateTicketStatusAsync_TicketExists_DispatchesStatusChangedEvent()
    {
        // Arrange
        var ticketId = 123;
        var oldStatus = TicketStatus.Open;
        var newStatus = TicketStatus.InProgress;
        var ticket = new Ticket
        {
            Id = ticketId,
            Status = oldStatus,
            AuthorId = 1,
            Title = "Test Status Update"
        };

        _mockTicketRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockDispatcher.Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _ticketService.UpdateTicketStatusAsync(ticketId, newStatus, actorUserId: 9);

        // Assert
        result.Should().Be(TicketUpdateResult.Updated);
        ticket.Status.Should().Be(newStatus);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

        // Verify TicketStatusChangedEvent was dispatched
        _mockDispatcher.Verify(d => d.DispatchAsync(It.Is<IEnumerable<IDomainEvent>>(events =>
            events.Any(e => e is TicketStatusChangedEvent)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateTicketStatusAsync_RecordsTheActingUser_NotTheAssignedDeveloper()
    {
        var ticket = new Ticket { Id = 5, Status = TicketStatus.Open, AuthorId = 1, DeveloperId = 2, Title = "Actor" };
        _mockTicketRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(ticket);

        TicketStatusChangedEvent? raised = null;
        _mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<IDomainEvent>, CancellationToken>((events, _) => raised ??= events.OfType<TicketStatusChangedEvent>().FirstOrDefault())
            .Returns(Task.CompletedTask);

        await _ticketService.UpdateTicketStatusAsync(5, TicketStatus.Closed, actorUserId: 3);

        raised.Should().NotBeNull();
        raised!.ActorUserId.Should().Be(3);
    }

    [Test]
    public async Task UpdateTicketPriorityAsync_TicketExists_UpdatesPriorityAndRecalculatesDueDate()
    {
        // Arrange
        var ticketId = 123;
        var ticket = new Ticket
        {
            Id = ticketId,
            Priority = TicketPriority.Medium,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            IsPriorityAssessed = false,
            IsSlaBreached = true,
            SlaWarningSent = true
        };

        _mockTicketRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockRealtimeNotifier.Setup(n => n.NotifyTicketsChangedAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _ticketService.UpdateTicketPriorityAsync(ticketId, TicketPriority.Critical);

        // Assert
        result.Should().Be(TicketUpdateResult.Updated);
        ticket.Priority.Should().Be(TicketPriority.Critical);
        ticket.IsPriorityAssessed.Should().BeTrue();
        ticket.IsSlaBreached.Should().BeFalse();
        ticket.SlaWarningSent.Should().BeFalse();

        // Critical priority SLA duration is 4 hours
        var timeDifference = ticket.DueDate!.Value - ticket.CreatedAt;
        timeDifference.TotalHours.Should().BeApproximately(4, 0.1);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockRealtimeNotifier.Verify(n => n.NotifyTicketsChangedAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockAuditService.Verify(a => a.LogActionAsync(
            "UpdateTicketPriority",
            "Ticket",
            ticketId.ToString(),
            It.Is<string>(c => c.Contains("Medium") && c.Contains("Critical")),
            null
        ), Times.Once);
    }

    [Test]
    public async Task CreateTicketAsync_WithPriorityAssessedFalse_PersistsFlagCorrectly()
    {
        // Arrange
        var ticket = Ticket.Create(
            "Unassessed ticket",
            "Description",
            TicketType.Project,
            TicketPriority.Medium,
            1,
            null,
            isPriorityAssessed: false);

        ticket.IsPriorityAssessed.Should().BeFalse();

        _mockTicketRepo.Setup(r => r.CreateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockDispatcher.Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _ticketService.CreateTicketAsync(ticket);

        // Assert
        result.IsPriorityAssessed.Should().BeFalse();
        _mockTicketRepo.Verify(r => r.CreateAsync(ticket), Times.Once);
    }

    [Test]
    public async Task GetStatusHistoryAsync_ReturnsTheTicketsHistoryFromTheRepository()
    {
        // Arrange
        var history = new Mock<ITicketStatusHistoryRepository>();
        var rows = new List<TicketStatusHistory> { new() { TicketId = 42, OldStatus = TicketStatus.Open, NewStatus = TicketStatus.InProgress } };
        history.Setup(h => h.GetByTicketAsync(42)).ReturnsAsync(rows);
        _mockFacade.Setup(f => f.TicketStatusHistory).Returns(history.Object);

        // Act
        var result = await _ticketService.GetStatusHistoryAsync(42);

        // Assert
        result.Should().BeSameAs(rows);
    }
}
