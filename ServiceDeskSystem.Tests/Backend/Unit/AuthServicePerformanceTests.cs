using FluentAssertions;
using Moq;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using NUnit.Framework;

namespace ServiceDeskSystem.Tests.Backend.Unit;

public class AuthServicePerformanceTests
{
    private Mock<IRepositoryFacadeFactory> _mockFactory;
    private Mock<IRepositoryFacade> _mockFacade;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<IContactInfoRepository> _mockContactInfoRepo;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IContactTypeRepository> _mockContactTypeRepo;

    private Mock<IPersonRepository> _mockPersonRepo;

    [SetUp]
    public void Setup()
    {
        _mockFactory = new Mock<IRepositoryFacadeFactory>();
        _mockFacade = new Mock<IRepositoryFacade>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockContactInfoRepo = new Mock<IContactInfoRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockContactTypeRepo = new Mock<IContactTypeRepository>();
        _mockPersonRepo = new Mock<IPersonRepository>();

        _mockFactory.Setup(f => f.Create()).Returns(_mockFacade.Object);
        _mockFacade.Setup(f => f.Users).Returns(_mockUserRepo.Object);
        _mockFacade.Setup(f => f.ContactInfos).Returns(_mockContactInfoRepo.Object);
        _mockFacade.Setup(f => f.ContactTypes).Returns(_mockContactTypeRepo.Object);
        _mockFacade.Setup(f => f.People).Returns(_mockPersonRepo.Object);
        _mockFacade.Setup(f => f.UnitOfWork).Returns(_mockUnitOfWork.Object);
    }

    [Test]
    public async Task RegisterClientAsync_UsesOptimizedQueries_NotGetAllAsync()
    {
        // Arrange
        var authService = new AuthService(_mockFactory.Object, null, null);

        _mockUserRepo.Setup(r => r.GetByLoginAsync("testuser")).ReturnsAsync((User?)null);
        
        var emailContactType = new ContactType { Id = 1, Name = "Email" };
        var contactTypes = new List<ContactType> { emailContactType };
        
        _mockContactTypeRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(contactTypes);
        _mockContactInfoRepo.Setup(r => r.ExistsByEmailAsync("test@test.com", 1)).ReturnsAsync(false);
        
        // Act
        var result = await authService.RegisterClientAsync("testuser", "password123", "Test", "User", "test@test.com");

        // Assert
        result.Success.Should().BeTrue();
        
        // Verify GetAllAsync is NOT called on Users
        _mockUserRepo.Verify(r => r.GetAllAsync(), Times.Never);
        
        // Verify GetByLoginAsync IS called
        _mockUserRepo.Verify(r => r.GetByLoginAsync("testuser"), Times.Once);

        // Verify ExistsByEmailAsync IS called
        _mockContactInfoRepo.Verify(r => r.ExistsByEmailAsync("test@test.com", 1), Times.Once);
    }
}
