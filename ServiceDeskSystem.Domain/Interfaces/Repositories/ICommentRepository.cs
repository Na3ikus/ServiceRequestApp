using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Domain.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<Comment?> GetByIdWithAuthorAsync(int id);

    /// <summary>Times (UTC) of comments written by the user at or after <paramref name="sinceUtc"/>.</summary>
    Task<List<DateTime>> GetCreatedTimesByAuthorAsync(int authorId, DateTime sinceUtc);
}
