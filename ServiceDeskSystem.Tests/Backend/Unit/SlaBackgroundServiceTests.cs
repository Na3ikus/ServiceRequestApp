using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Notifications;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class SlaBackgroundServiceTests
{
    private Mock<IServiceProvider> _mockServiceProvider;
    private Mock<IServiceScope> _mockScope;
    private Mock<IServiceProvider> _mockScopeServiceProvider;
    private Mock<IServiceScopeFactory> _mockScopeFactory;
    private Mock<IRepositoryFacadeFactory> _mockRepoFacadeFactory;
    private Mock<IRepositoryFacade> _mockRepoFacade;
    private Mock<ITicketRepository> _mockTicketRepo;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<INotificationService> _mockNotificationService;
    private Mock<IRealtimeNotifier> _mockRealtimeNotifier;
    private Mock<IEmailSender> _mockEmailSender;
    private Mock<ILogger<SlaBackgroundService>> _mockLogger;
    private SlaBackgroundService _slaService;

    [SetUp]
    public void SetUp()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockScope = new Mock<IServiceScope>();
        _mockScopeServiceProvider = new Mock<IServiceProvider>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockRepoFacadeFactory = new Mock<IRepositoryFacadeFactory>();
        _mockRepoFacade = new Mock<IRepositoryFacade>();
        _mockTicketRepo = new Mock<ITicketRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockRealtimeNotifier = new Mock<IRealtimeNotifier>();
        _mockEmailSender = new Mock<IEmailSender>();
        _mockLogger = new Mock<ILogger<SlaBackgroundService>>();

        // Setup service provider scope resolution
        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockScopeServiceProvider.Object);
        _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockScopeFactory.Object);

        // Setup resolved dependencies
        _mockScopeServiceProvider.Setup(sp => sp.GetService(typeof(IRepositoryFacadeFactory)))
            .Returns(_mockRepoFacadeFactory.Object);
        _mockScopeServiceProvider.Setup(sp => sp.GetService(typeof(INotificationService)))
            .Returns(_mockNotificationService.Object);
        _mockScopeServiceProvider.Setup(sp => sp.GetService(typeof(IRealtimeNotifier)))
            .Returns(_mockRealtimeNotifier.Object);
        _mockScopeServiceProvider.Setup(sp => sp.GetService(typeof(IEmailSender)))
            .Returns(_mockEmailSender.Object);

        // Setup repository facade
        _mockRepoFacadeFactory.Setup(f => f.Create()).Returns(_mockRepoFacade.Object);
        _mockRepoFacade.Setup(f => f.Tickets).Returns(_mockTicketRepo.Object);
        _mockRepoFacade.Setup(f => f.UnitOfWork).Returns(_mockUnitOfWork.Object);

        _slaService = new SlaBackgroundService(_mockServiceProvider.Object, _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _slaService.Dispose();
    }

    [Test]
    public async Task CheckSlaAsync_NoActiveTickets_DoesNothing()
    {
        // Arrange
        _mockTicketRepo.Setup(r => r.GetActiveTicketsForSlaAsync())
            .ReturnsAsync(new List<Ticket>());

        // Act
        await _slaService.CheckSlaAsync(CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        _mockRealtimeNotifier.Verify(n => n.NotifyTicketsChangedAsync(), Times.Never);
    }

    [Test]
    public async Task CheckSlaAsync_TicketOverdue_TriggersBreach()
    {
        // Arrange
        var overdueTicket = new Ticket
        {
            Id = 1,
            Title = "Overdue Ticket",
            DueDate = DateTime.UtcNow.AddMinutes(-5), // 5 minutes in the past
            IsSlaBreached = false,
            SlaWarningSent = false,
            AuthorId = 2,
            Author = new User { Id = 2, Login = "author" }
        };

        _mockTicketRepo.Setup(r => r.GetActiveTicketsForSlaAsync())
            .ReturnsAsync(new List<Ticket> { overdueTicket });
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockRealtimeNotifier.Setup(n => n.NotifyTicketsChangedAsync()).Returns(Task.CompletedTask);

        // Act
        await _slaService.CheckSlaAsync(CancellationToken.None);

        // Assert
        overdueTicket.IsSlaBreached.Should().BeTrue();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockRealtimeNotifier.Verify(n => n.NotifyTicketsChangedAsync(), Times.Once);

        // Verify in-app notification is created for the author
        _mockNotificationService.Verify(n => n.CreateSlaNotificationAsync(
            overdueTicket.Id,
            "SlaBreached",
            It.Is<string>(msg => msg.Contains("BREACHED")),
            overdueTicket.AuthorId), Times.Once);
    }

    [Test]
    public async Task CheckSlaAsync_TicketApproachingDueDate_TriggersWarning()
    {
        // Arrange
        var warningTicket = new Ticket
        {
            Id = 1,
            Title = "Warning Ticket",
            DueDate = DateTime.UtcNow.AddHours(23), // 23 hours in the future (Medium priority warning threshold is 24 hours)
            IsSlaBreached = false,
            SlaWarningSent = false,
            Priority = TicketPriority.Medium,
            AuthorId = 2,
            Author = new User { Id = 2, Login = "author" }
        };

        _mockTicketRepo.Setup(r => r.GetActiveTicketsForSlaAsync())
            .ReturnsAsync(new List<Ticket> { warningTicket });
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockRealtimeNotifier.Setup(n => n.NotifyTicketsChangedAsync()).Returns(Task.CompletedTask);

        // Act
        await _slaService.CheckSlaAsync(CancellationToken.None);

        // Assert
        warningTicket.SlaWarningSent.Should().BeTrue();
        warningTicket.IsSlaBreached.Should().BeFalse();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockRealtimeNotifier.Verify(n => n.NotifyTicketsChangedAsync(), Times.Once);

        // Verify in-app notification is created for the author
        _mockNotificationService.Verify(n => n.CreateSlaNotificationAsync(
            warningTicket.Id,
            "SlaWarning",
            It.Is<string>(msg => msg.Contains("WARNING")),
            warningTicket.AuthorId), Times.Once);
    }

    [Test]
    public async Task CheckSlaAsync_WarningAlreadySent_DoesNotTriggerDuplicateWarning()
    {
        // Arrange
        var ticket = new Ticket
        {
            Id = 1,
            Title = "Warning Sent Ticket",
            DueDate = DateTime.UtcNow.AddHours(23),
            IsSlaBreached = false,
            SlaWarningSent = true, // Warning already sent!
            Priority = TicketPriority.Medium,
            AuthorId = 2
        };

        _mockTicketRepo.Setup(r => r.GetActiveTicketsForSlaAsync())
            .ReturnsAsync(new List<Ticket> { ticket });

        // Act
        await _slaService.CheckSlaAsync(CancellationToken.None);

        // Assert
        ticket.SlaWarningSent.Should().BeTrue();
        ticket.IsSlaBreached.Should().BeFalse();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        _mockNotificationService.Verify(n => n.CreateSlaNotificationAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }
}
