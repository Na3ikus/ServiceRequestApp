using ServiceDeskSystem.Application.Services.Comments.Interfaces;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Infrastructure.Data;
using ServiceDeskSystem.Infrastructure.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace ServiceDeskSystem.Application.Services.Comments;

/// <summary>
/// Service for CRUD operations on ticket comments.
/// </summary>
public sealed class CommentService(IDbContextFactory<BugTrackerDbContext> contextFactory) : ICommentService
{
    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        await using var repo = new RepositoryFacade(contextFactory);
        comment.CreatedAt = DateTime.UtcNow;

        await repo.Comments.CreateAsync(comment).ConfigureAwait(false);
        await repo.SaveChangesAsync().ConfigureAwait(false);

        var result = await repo.Comments.GetByIdWithAuthorAsync(comment.Id).ConfigureAwait(false);
        return result ?? comment;
    }

    public async Task<Comment?> UpdateCommentAsync(int commentId, string newMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newMessage);

        await using var repo = new RepositoryFacade(contextFactory);
        var existing = await repo.Comments.GetByIdWithAuthorAsync(commentId).ConfigureAwait(false);

        if (existing is null)
        {
            return null;
        }

        existing.Message = newMessage;
        await repo.SaveChangesAsync().ConfigureAwait(false);
        return existing;
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        await using var repo = new RepositoryFacade(contextFactory);
        var comment = await repo.Comments.GetByIdAsync(commentId).ConfigureAwait(false);

        if (comment is null)
        {
            return false;
        }

        await repo.Comments.DeleteAsync(commentId).ConfigureAwait(false);
        await repo.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }
}
