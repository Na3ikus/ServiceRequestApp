using FluentAssertions;
using Moq;
using ServiceDeskSystem.Application.Services.Notifications;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using NUnit.Framework;

namespace ServiceDeskSystem.Tests.Backend.Unit;

public class NotificationServiceTests
{
    private readonly Mock<IRepositoryFacadeFactory> _mockFactory;
    private readonly Mock<IRepositoryFacade> _mockFacade;
    private readonly Mock<INotificationRepository> _mockNotifRepo;
    private readonly Mock<IRealtimeNotifier> _mockNotifier;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public NotificationServiceTests()
    {
        _mockFactory = new Mock<IRepositoryFacadeFactory>();
        _mockFacade = new Mock<IRepositoryFacade>();
        _mockNotifRepo = new Mock<INotificationRepository>();
        _mockNotifier = new Mock<IRealtimeNotifier>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockFactory.Setup(f => f.Create()).Returns(_mockFacade.Object);
        _mockFacade.Setup(f => f.Notifications).Returns(_mockNotifRepo.Object);
        _mockFacade.Setup(f => f.UnitOfWork).Returns(_mockUnitOfWork.Object);
    }

    [Test]
    public async Task GetUnreadCountAsync_ReturnsCorrectCount()
    {
        var service = new NotificationService(_mockFactory.Object, _mockNotifier.Object);
        _mockNotifRepo.Setup(r => r.GetUnreadCountAsync(1)).ReturnsAsync(5);

        var result = await service.GetUnreadCountAsync(1);

        result.Should().Be(5);
    }
}

