using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Application.Services.Notifications;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Common;
using ServiceDeskSystem.Domain.Common;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Domain.Interfaces.Repositories;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class TicketStatisticsTests
{
    private Mock<IRepositoryFacadeFactory> _mockFactory;
    private Mock<IRepositoryFacade> _mockFacade;
    private Mock<ITicketRepository> _mockTicketRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<IWorkLogRepository> _mockWorkLogRepo;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<INotificationService> _mockNotificationService;
    private Mock<IRealtimeNotifier> _mockRealtimeNotifier;
    private Mock<IDomainEventDispatcher> _mockDispatcher;
    private Mock<IAuditService> _mockAuditService;
    private IMemoryCache _memoryCache;

    private TicketService _ticketService;

    [SetUp]
    public void SetUp()
    {
        _mockFactory = new Mock<IRepositoryFacadeFactory>();
        _mockFacade = new Mock<IRepositoryFacade>();
        _mockTicketRepo = new Mock<ITicketRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockWorkLogRepo = new Mock<IWorkLogRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockRealtimeNotifier = new Mock<IRealtimeNotifier>();
        _mockDispatcher = new Mock<IDomainEventDispatcher>();
        _mockAuditService = new Mock<IAuditService>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _mockFacade.Setup(f => f.Tickets).Returns(_mockTicketRepo.Object);
        _mockFacade.Setup(f => f.Users).Returns(_mockUserRepo.Object);
        _mockFacade.Setup(f => f.WorkLogs).Returns(_mockWorkLogRepo.Object);
        _mockFacade.Setup(f => f.UnitOfWork).Returns(_mockUnitOfWork.Object);

        _mockFactory.Setup(f => f.Create()).Returns(_mockFacade.Object);

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
        _memoryCache?.Dispose();
    }

    [Test]
    public async Task GetEmployeeEfficiencyAsync_ShouldCalculateMetricsCorrectly()
    {
        // Arrange
        var dev = new User { Id = 1, Login = "dev1", Role = UserRole.Developer };
        var activeDevs = new List<User> { dev };

        var devTickets = new List<Ticket>
        {
            new Ticket { Id = 1, Status = TicketStatus.Closed, DeveloperId = 1 },
            new Ticket { Id = 2, Status = TicketStatus.InProgress, DeveloperId = 1 },
            new Ticket { Id = 3, Status = TicketStatus.Closed, DeveloperId = 1 }
        };

        _mockUserRepo.Setup(r => r.GetAllWithPersonAsync())
            .ReturnsAsync(activeDevs);

        _mockTicketRepo.Setup(r => r.GetAllWithIncludesAsync())
            .ReturnsAsync(devTickets);

        _mockWorkLogRepo.Setup(r => r.GetTotalTimeSpentForUserAsync(1))
            .ReturnsAsync(180); // 3 hours

        // Act
        var result = await _ticketService.GetEmployeeEfficiencyAsync(30);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        var stats = result.First();
        stats.DeveloperName.Should().Be("dev1");
        stats.TicketsAssigned.Should().Be(3);
        stats.TicketsClosed.Should().Be(2);
        stats.ClosureRatePercentage.Should().Be(66.7); // 2/3 * 100 = 66.7%
        stats.TotalTimeSpentMinutes.Should().Be(180);
    }
}
