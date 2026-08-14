using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Application.Services.Tags;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class TagServiceTests
{
    private Mock<IRepositoryFacadeFactory> _mockFactory;
    private Mock<IRepositoryFacade> _mockFacade;
    private Mock<IAuditService> _mockAuditService;
    private Mock<IRealtimeNotifier> _mockRealtimeNotifier;
    private Mock<ITagRepository> _mockTagRepo;
    private Mock<ITicketRepository> _mockTicketRepo;
    private Mock<IUnitOfWork> _mockUnitOfWork;

    private TagService _tagService;

    [SetUp]
    public void SetUp()
    {
        _mockFactory = new Mock<IRepositoryFacadeFactory>();
        _mockFacade = new Mock<IRepositoryFacade>();
        _mockAuditService = new Mock<IAuditService>();
        _mockRealtimeNotifier = new Mock<IRealtimeNotifier>();

        _mockTagRepo = new Mock<ITagRepository>();
        _mockTicketRepo = new Mock<ITicketRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockFacade.Setup(f => f.Tags).Returns(_mockTagRepo.Object);
        _mockFacade.Setup(f => f.Tickets).Returns(_mockTicketRepo.Object);
        _mockFacade.Setup(f => f.UnitOfWork).Returns(_mockUnitOfWork.Object);

        _mockFactory.Setup(f => f.Create()).Returns(_mockFacade.Object);

        _tagService = new TagService(_mockFactory.Object, _mockRealtimeNotifier.Object, _mockAuditService.Object);
    }

    [Test]
    public async Task CreateTagAsync_ShouldCreateTag_WhenNameIsValid()
    {
        // Arrange
        var tagName = "Urgent";
        var tagColor = "#FF0000";

        // Act
        var result = await _tagService.CreateTagAsync(tagName, tagColor, 1);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(tagName);
        result.Color.Should().Be(tagColor);

        _mockTagRepo.Verify(r => r.CreateAsync(It.IsAny<Tag>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateTagAsync_ShouldThrowException_WhenNameIsNullOrWhitespace()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await _tagService.CreateTagAsync("", "#000"));
        Assert.ThrowsAsync<ArgumentNullException>(async () => await _tagService.CreateTagAsync(null!, "#000"));
    }

    [Test]
    public async Task UpdateTagAsync_ShouldUpdateTag_WhenFound()
    {
        // Arrange
        var tag = new Tag { Id = 1, Name = "Old Name", Color = "#000" };
        _mockTagRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);

        // Act
        var result = await _tagService.UpdateTagAsync(1, "New Name", "#FFF", 1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.Color.Should().Be("#FFF");

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateTagAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _mockTagRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Tag)null!);

        // Act
        var result = await _tagService.UpdateTagAsync(1, "New Name", "#FFF");

        // Assert
        result.Should().BeNull();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task DeleteTagAsync_ShouldDeleteTag_WhenFound()
    {
        // Arrange
        var tag = new Tag { Id = 1, Name = "Test" };
        _mockTagRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);

        // Act
        var result = await _tagService.DeleteTagAsync(1, 1);

        // Assert
        result.Should().BeTrue();
        _mockTagRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
