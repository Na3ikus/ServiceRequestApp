using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Application.Services.Comments;
using ServiceDeskSystem.Application.Services.Notifications;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class CommentServiceTests
{
    private Mock<IRepositoryFacadeFactory> _mockFactory;
    private Mock<IRepositoryFacade> _mockFacade;
    private Mock<ICommentRepository> _mockCommentRepo;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<INotificationService> _mockNotificationService;
    private Mock<IRealtimeNotifier> _mockRealtimeNotifier;
    private Mock<IAuditService> _mockAuditService;
    private CommentService _commentService;

    [SetUp]
    public void SetUp()
    {
        _mockFactory = new Mock<IRepositoryFacadeFactory>();
        _mockFacade = new Mock<IRepositoryFacade>();
        _mockCommentRepo = new Mock<ICommentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockRealtimeNotifier = new Mock<IRealtimeNotifier>();
        _mockAuditService = new Mock<IAuditService>();

        _mockFactory.Setup(f => f.Create()).Returns(_mockFacade.Object);
        _mockFacade.Setup(f => f.Comments).Returns(_mockCommentRepo.Object);
        _mockFacade.Setup(f => f.UnitOfWork).Returns(_mockUnitOfWork.Object);

        _commentService = new CommentService(
            _mockFactory.Object,
            _mockNotificationService.Object,
            _mockRealtimeNotifier.Object,
            _mockAuditService.Object);
    }

    [Test]
    public async Task AddCommentAsync_ValidComment_SavesAndNotifies()
    {
        // Arrange
        var comment = new Comment
        {
            TicketId = 10,
            AuthorId = 2,
            Message = "This is a comment"
        };

        _mockCommentRepo.Setup(r => r.CreateAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockNotificationService.Setup(n => n.CreateCommentNotificationAsync(comment.TicketId, comment.AuthorId)).Returns(Task.CompletedTask);
        _mockRealtimeNotifier.Setup(n => n.NotifyTicketsChangedAsync()).Returns(Task.CompletedTask);
        _mockAuditService.Setup(a => a.LogActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>())).Returns(Task.CompletedTask);

        // Act
        var result = await _commentService.AddCommentAsync(comment);

        // Assert
        result.Should().NotBeNull();
        _mockCommentRepo.Verify(r => r.CreateAsync(comment), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockNotificationService.Verify(n => n.CreateCommentNotificationAsync(comment.TicketId, comment.AuthorId), Times.Once);
        _mockRealtimeNotifier.Verify(n => n.NotifyTicketsChangedAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateCommentAsync_CommentDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockCommentRepo.Setup(r => r.GetByIdWithAuthorAsync(1)).ReturnsAsync((Comment?)null);

        // Act
        var result = await _commentService.UpdateCommentAsync(1, "Updated Message", 2, false);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task UpdateCommentAsync_NotAuthorOrAdmin_ReturnsForbidden()
    {
        // Arrange
        var comment = new Comment { Id = 1, AuthorId = 3, Message = "Original Message" };
        _mockCommentRepo.Setup(r => r.GetByIdWithAuthorAsync(1)).ReturnsAsync(comment);

        // Act
        var result = await _commentService.UpdateCommentAsync(1, "Updated Message", 2, false); // Requester is 2, author is 3, not admin

        // Assert
        result.Should().Be(CommentService.Forbidden);
    }

    [Test]
    public async Task UpdateCommentAsync_IsAuthor_Succeeds()
    {
        // Arrange
        var comment = new Comment { Id = 1, AuthorId = 2, Message = "Original Message" };
        _mockCommentRepo.Setup(r => r.GetByIdWithAuthorAsync(1)).ReturnsAsync(comment);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _commentService.UpdateCommentAsync(1, "Updated Message", 2, false); // Requester is 2, author is 2, not admin

        // Assert
        result.Should().NotBeNull();
        result!.Message.Should().Be("Updated Message");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateCommentAsync_IsAdmin_Succeeds()
    {
        // Arrange
        var comment = new Comment { Id = 1, AuthorId = 3, Message = "Original Message" };
        _mockCommentRepo.Setup(r => r.GetByIdWithAuthorAsync(1)).ReturnsAsync(comment);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _commentService.UpdateCommentAsync(1, "Updated Message", 2, true); // Requester is 2, author is 3, but IS admin

        // Assert
        result.Should().NotBeNull();
        result!.Message.Should().Be("Updated Message");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task AddCommentAsync_ShouldCreateInternalNote_WhenIsInternalIsTrue()
    {
        // Arrange
        var comment = new Comment
        {
            TicketId = 10,
            AuthorId = 2,
            Message = "This is an internal note",
            IsInternal = true
        };

        _mockCommentRepo.Setup(r => r.CreateAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockNotificationService.Setup(n => n.CreateCommentNotificationAsync(comment.TicketId, comment.AuthorId)).Returns(Task.CompletedTask);
        _mockRealtimeNotifier.Setup(n => n.NotifyTicketsChangedAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _commentService.AddCommentAsync(comment);

        // Assert
        result.Should().NotBeNull();
        result.IsInternal.Should().BeTrue();
        _mockCommentRepo.Verify(r => r.CreateAsync(It.Is<Comment>(c => c.IsInternal)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
