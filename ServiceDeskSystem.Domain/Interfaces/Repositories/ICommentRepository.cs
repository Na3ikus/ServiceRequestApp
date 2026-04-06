using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Domain.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<Comment?> GetByIdWithAuthorAsync(int id);
}
