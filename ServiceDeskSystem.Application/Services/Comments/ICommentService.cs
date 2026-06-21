using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.Comments;

/// <summary>
/// Provides CRUD operations for ticket comments.
/// Comment is a separate domain entity and should not live inside ITicketService.
/// </summary>
public interface ICommentService
{
    Task<Comment> AddCommentAsync(Comment comment);

    /// <summary>
    /// Updates the text of an existing comment.
    /// Returns null if the comment was not found.
    /// Returns <see cref="CommentService.Forbidden"/> if the requester does not own the comment and is not an Admin.
    /// </summary>
    Task<Comment?> UpdateCommentAsync(int commentId, string newMessage, int? requesterId, bool isAdmin);

    Task<bool> DeleteCommentAsync(int commentId);
}

