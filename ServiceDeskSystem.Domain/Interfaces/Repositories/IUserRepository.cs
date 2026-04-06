using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByLoginAsync(string login);

    Task<IEnumerable<User>> GetAllWithPersonAsync();
}
