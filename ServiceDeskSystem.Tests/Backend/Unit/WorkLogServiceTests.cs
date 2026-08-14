using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Application.Services.WorkLogs;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Domain.Interfaces.Repositories;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class WorkLogServiceTests
{
    private Mock<IRepositoryFacadeFactory> _mockFactory;
    private Mock<IRepositoryFacade> _mockFacade;
    private Mock<IAuditService> _mockAuditService;
    private Mock<ITicketRepository> _mockTicketRepo;
    private Mock<IWorkLogRepository> _mockWorkLogRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<IUnitOfWork> _mockUnitOfWork;

    private WorkLogService _workLogService;

    [SetUp]
    public void SetUp()
    {
        _mockFactory = new Mock<IRepositoryFacadeFactory>();
        _mockFacade = new Mock<IRepositoryFacade>();
        _mockAuditService = new Mock<IAuditService>();

        _mockTicketRepo = new Mock<ITicketRepository>();
        _mockWorkLogRepo = new Mock<IWorkLogRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockFacade.Setup(f => f.Tickets).Returns(_mockTicketRepo.Object);
        _mockFacade.Setup(f => f.WorkLogs).Returns(_mockWorkLogRepo.Object);
        _mockFacade.Setup(f => f.Users).Returns(_mockUserRepo.Object);
        _mockFacade.Setup(f => f.UnitOfWork).Returns(_mockUnitOfWork.Object);

        _mockFactory.Setup(f => f.Create()).Returns(_mockFacade.Object);

        _workLogService = new WorkLogService(_mockFactory.Object, _mockAuditService.Object);
    }

    [Test]
    public async Task AddWorkLogAsync_ShouldReturnSuccess_WhenValidData()
    {
        // Arrange
        var ticketId = 1;
        var userId = 2;
        var timeSpent = 60;
        var date = DateTime.UtcNow;
        var desc = "Test work log";

        _mockTicketRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(new Ticket { Id = ticketId });
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });

        // Act
        var result = await _workLogService.AddWorkLogAsync(ticketId, userId, timeSpent, date, desc);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();

        _mockWorkLogRepo.Verify(r => r.CreateAsync(It.Is<WorkLog>(w => w.TimeSpentMinutes == timeSpent && w.Description == desc)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task AddWorkLogAsync_ShouldFail_WhenTimeIsZeroOrNegative()
    {
        // Arrange
        var ticketId = 1;
        var userId = 2;

        // Act
        var resultZero = await _workLogService.AddWorkLogAsync(ticketId, userId, 0, DateTime.UtcNow, "desc");
        var resultNegative = await _workLogService.AddWorkLogAsync(ticketId, userId, -10, DateTime.UtcNow, "desc");

        // Assert
        resultZero.Success.Should().BeFalse();
        resultNegative.Success.Should().BeFalse();

        _mockWorkLogRepo.Verify(r => r.CreateAsync(It.IsAny<WorkLog>()), Times.Never);
    }

    [Test]
    public async Task AddWorkLogAsync_ShouldFail_WhenDescriptionIsEmpty()
    {
        // Arrange
        var ticketId = 1;
        var userId = 2;

        // Act
        var result = await _workLogService.AddWorkLogAsync(ticketId, userId, 60, DateTime.UtcNow, "");

        // Assert
        result.Success.Should().BeFalse();
        _mockWorkLogRepo.Verify(r => r.CreateAsync(It.IsAny<WorkLog>()), Times.Never);
    }
}
