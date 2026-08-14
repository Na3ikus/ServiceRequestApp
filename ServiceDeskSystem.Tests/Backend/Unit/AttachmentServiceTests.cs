using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Attachments;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Domain.Interfaces.Repositories;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class AttachmentServiceTests
{
    private Mock<IRepositoryFacadeFactory> _mockFactory;
    private Mock<IRepositoryFacade> _mockFacade;
    private Mock<IAuditService> _mockAuditService;
    private Mock<IWebHostEnvironment> _mockEnv;
    private Mock<ITicketRepository> _mockTicketRepo;
    private Mock<IAttachmentRepository> _mockAttachmentRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<IUnitOfWork> _mockUnitOfWork;

    private AttachmentService _attachmentService;
    private string _tempWebRoot;

    [SetUp]
    public void SetUp()
    {
        _mockFactory = new Mock<IRepositoryFacadeFactory>();
        _mockFacade = new Mock<IRepositoryFacade>();
        _mockAuditService = new Mock<IAuditService>();
        _mockEnv = new Mock<IWebHostEnvironment>();

        _mockTicketRepo = new Mock<ITicketRepository>();
        _mockAttachmentRepo = new Mock<IAttachmentRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockFacade.Setup(f => f.Tickets).Returns(_mockTicketRepo.Object);
        _mockFacade.Setup(f => f.Attachments).Returns(_mockAttachmentRepo.Object);
        _mockFacade.Setup(f => f.Users).Returns(_mockUserRepo.Object);
        _mockFacade.Setup(f => f.UnitOfWork).Returns(_mockUnitOfWork.Object);

        _mockFactory.Setup(f => f.Create()).Returns(_mockFacade.Object);

        _tempWebRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempWebRoot);
        _mockEnv.Setup(e => e.WebRootPath).Returns(_tempWebRoot);

        _attachmentService = new AttachmentService(_mockFactory.Object, _mockAuditService.Object, _mockEnv.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempWebRoot))
        {
            Directory.Delete(_tempWebRoot, true);
        }
    }

    [Test]
    public async Task UploadAttachmentAsync_ShouldReturnSuccess_WhenValidData()
    {
        // Arrange
        var ticketId = 1;
        var userId = 2;
        var fileName = "test.png";
        var content = "dummy image content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        _mockTicketRepo.Setup(r => r.GetByIdAsync(ticketId))
            .ReturnsAsync(new Ticket { Id = ticketId });

        // Act
        var result = await _attachmentService.UploadAttachmentAsync(ticketId, fileName, "image/png", stream, userId);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.Attachment.Should().NotBeNull();
        result.Attachment!.FileName.Should().Be(fileName);

        _mockAttachmentRepo.Verify(r => r.CreateAsync(It.IsAny<Attachment>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UploadAttachmentAsync_ShouldFail_WhenFileTooLarge()
    {
        // Arrange
        var ticketId = 1;
        var userId = 2;
        var fileName = "test.png";
        var stream = new Mock<Stream>();
        stream.Setup(s => s.Length).Returns(20 * 1024 * 1024); // 20 MB

        // Act
        var result = await _attachmentService.UploadAttachmentAsync(ticketId, fileName, "image/png", stream.Object, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("File size exceeds");
    }

    [Test]
    public async Task UploadAttachmentAsync_ShouldFail_WhenExtensionNotAllowed()
    {
        // Arrange
        var ticketId = 1;
        var userId = 2;
        var fileName = "test.exe";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("dummy"));

        // Act
        var result = await _attachmentService.UploadAttachmentAsync(ticketId, fileName, "application/octet-stream", stream, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("File type is not allowed.");
    }

    [Test]
    public async Task DeleteAttachmentAsync_ShouldReturnSuccess_WhenUserIsOwner()
    {
        // Arrange
        var attachmentId = 1;
        var userId = 2;

        var attachment = new Attachment { Id = attachmentId, UploadedById = userId, FilePath = "test.png" };
        var user = new User { Id = userId, Role = UserRole.User };

        _mockAttachmentRepo.Setup(r => r.GetByIdAsync(attachmentId)).ReturnsAsync(attachment);
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _attachmentService.DeleteAttachmentAsync(attachmentId, userId);

        // Assert
        result.Success.Should().BeTrue();
        _mockAttachmentRepo.Verify(r => r.DeleteAsync(attachmentId), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteAttachmentAsync_ShouldFail_WhenUserIsNotOwnerAndNotAdmin()
    {
        // Arrange
        var attachmentId = 1;
        var ownerId = 2;
        var otherUserId = 3;

        var attachment = new Attachment { Id = attachmentId, UploadedById = ownerId, FilePath = "test.png" };
        var otherUser = new User { Id = otherUserId, Role = UserRole.User };

        _mockAttachmentRepo.Setup(r => r.GetByIdAsync(attachmentId)).ReturnsAsync(attachment);
        _mockUserRepo.Setup(r => r.GetByIdAsync(otherUserId)).ReturnsAsync(otherUser);

        // Act
        var result = await _attachmentService.DeleteAttachmentAsync(attachmentId, otherUserId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Forbidden");
        _mockAttachmentRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
