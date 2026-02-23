using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.Comments.Interfaces;

/// <summary>
/// Provides CRUD operations for ticket comments.
/// Comment is a separate domain entity and should not live inside ITicketService.
/// </summary>
public interface ICommentService
{
    Task<Comment> AddCommentAsync(Comment comment);

    Task<Comment?> UpdateCommentAsync(int commentId, string newMessage);

    Task<bool> DeleteCommentAsync(int commentId);
}
