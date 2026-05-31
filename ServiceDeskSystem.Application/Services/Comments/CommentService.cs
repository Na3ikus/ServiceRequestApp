using ServiceDeskSystem.Application.Services.Comments.Interfaces;
using ServiceDeskSystem.Application.Services.Notifications.Interfaces;
using ServiceDeskSystem.Application.Services.Realtime.Interfaces;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Application.Services.Audit.Interfaces;

namespace ServiceDeskSystem.Application.Services.Comments;

/// <summary>
/// Service for CRUD operations on ticket comments.
/// </summary>
public sealed class CommentService(
    IRepositoryFacadeFactory repositoryFacadeFactory,
    INotificationService notificationService,
    IRealtimeNotifier realtimeNotifier,
    IAuditService? auditService = null) : ICommentService
{

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        await using var repo = repositoryFacadeFactory.Create();
        comment.CreatedAt = DateTime.UtcNow;

        await repo.Comments.CreateAsync(comment).ConfigureAwait(false);
        await repo.SaveChangesAsync().ConfigureAwait(false);

        await notificationService.CreateCommentNotificationAsync(comment.TicketId, comment.AuthorId).ConfigureAwait(false);
        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync("ADD_COMMENT", "Comment", comment.Id.ToString(), $"Added comment to ticket {comment.TicketId}", comment.AuthorId).ConfigureAwait(false);

        var result = await repo.Comments.GetByIdWithAuthorAsync(comment.Id).ConfigureAwait(false);
        return result ?? comment;
    }

    public async Task<Comment?> UpdateCommentAsync(int commentId, string newMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newMessage);

        await using var repo = repositoryFacadeFactory.Create();
        var existing = await repo.Comments.GetByIdWithAuthorAsync(commentId).ConfigureAwait(false);

        if (existing is null)
        {
            return null;
        }

        existing.Message = newMessage;
        await repo.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync("UPDATE_COMMENT", "Comment", commentId.ToString(), $"Updated comment in ticket {existing.TicketId}", existing.AuthorId).ConfigureAwait(false);

        return existing;
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var comment = await repo.Comments.GetByIdAsync(commentId).ConfigureAwait(false);

        if (comment is null)
        {
            return false;
        }

        await repo.Comments.DeleteAsync(commentId).ConfigureAwait(false);
        await repo.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync("DELETE_COMMENT", "Comment", commentId.ToString(), $"Deleted comment from ticket {comment.TicketId}", comment.AuthorId).ConfigureAwait(false);

        return true;
    }
}

