using FluentAssertions;
using Moq;
using ServiceDeskSystem.Application.Services.Profile;
using ServiceDeskSystem.Application.Services.Profile.Models;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using NUnit.Framework;

namespace ServiceDeskSystem.Tests.Backend.Unit;

public class ProfileServiceTests
{
    private readonly Mock<IRepositoryFacadeFactory> _mockFactory;
    private readonly Mock<IRepositoryFacade> _mockFacade;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public ProfileServiceTests()
    {
        _mockFactory = new Mock<IRepositoryFacadeFactory>();
        _mockFacade = new Mock<IRepositoryFacade>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockFactory.Setup(f => f.Create()).Returns(_mockFacade.Object);
        _mockFacade.Setup(f => f.Users).Returns(_mockUserRepo.Object);
        _mockFacade.Setup(f => f.UnitOfWork).Returns(_mockUnitOfWork.Object);
    }

    [Test]
    public async Task GetProfileAsync_UserNotFound_ReturnsNull()
    {
        var service = new ProfileService(_mockFactory.Object);
        _mockUserRepo.Setup(r => r.GetByIdWithPersonAndContactsAsync(1)).ReturnsAsync((User?)null);

        var result = await service.GetProfileAsync(1);

        result.Should().BeNull();
    }
}

