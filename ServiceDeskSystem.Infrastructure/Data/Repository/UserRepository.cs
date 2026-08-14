using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Infrastructure.Data.Repository
{
    public sealed class UserRepository : TemplateRepository<User>, IUserRepository
    {
        public UserRepository(BugTrackerDbContext context)
            : base(context)
        {
        }

        protected override DbSet<User> DbSet => this.Context.Users;

        public async Task<User?> GetByLoginAsync(string login)
        {
            var user = await this.Context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Login == login)
                .ConfigureAwait(false);

            if (user is not null && !string.Equals(user.Login, login, StringComparison.Ordinal))
            {
                return null;
            }

            return user;
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

        public async Task<User?> GetByIdWithPersonAndContactsAsync(int userId)
        {
            return await this.Context.Users
                .Include(u => u.Person)
                .ThenInclude(p => p.ContactInfos)
                .ThenInclude(ci => ci.ContactType)
                .FirstOrDefaultAsync(u => u.Id == userId)
                .ConfigureAwait(false);
        }
    }
}

