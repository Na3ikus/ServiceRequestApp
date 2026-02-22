using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Infrastructure.Data.Repository
{
    public sealed class UserRepository : TemplateRepository<User>
    {
        public UserRepository(BugTrackerDbContext context)
            : base(context)
        {
        }

        protected override DbSet<User> DbSet => this.Context.Users;

        public User? GetByLogin(string login)
        {
            return this.Context.Users
                .Include(u => u.Person)
                .FirstOrDefault(u => u.Login == login);
        }

        public async Task<User?> GetByLoginAsync(string login)
        {
            return await this.Context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Login == login)
                .ConfigureAwait(false);
        }

        public IEnumerable<User> GetAllWithPerson()
        {
            return this.Context.Users
                .Include(u => u.Person)
                .ToList();
        }

        public async Task<IEnumerable<User>> GetAllWithPersonAsync()
        {
            return await this.Context.Users
                .Include(u => u.Person)
                .OrderBy(u => u.Login)
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}

