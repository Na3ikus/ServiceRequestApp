using FluentAssertions;
using Moq;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using NUnit.Framework;

namespace ServiceDeskSystem.Tests.Backend.Unit;

public class AdminServiceTests
{
    private readonly Mock<IRepositoryFacadeFactory> _mockFactory;
    private readonly Mock<IRepositoryFacade> _mockFacade;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public AdminServiceTests()
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
    public async Task GetAllUsersAsync_ReturnsUserList()
    {
        var service = new AdminService(_mockFactory.Object);
        var users = new List<User> { new User { Id = 1, Login = "test" } };
        _mockUserRepo.Setup(r => r.GetAllWithPersonAsync()).ReturnsAsync(users);

        var result = await service.GetAllUsersAsync();

        result.Should().HaveCount(1);
        result.First().Login.Should().Be("test");
    }

    [Test]
    public async Task UpdateUserRoleAsync_ValidUser_ChangesRole()
    {
        var service = new AdminService(_mockFactory.Object);
        var user = new User { Id = 1, Role = UserRole.User };
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await service.UpdateUserRoleAsync(1, UserRole.Admin);

        result.Should().BeTrue();
        user.Role.Should().Be(UserRole.Admin);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}

