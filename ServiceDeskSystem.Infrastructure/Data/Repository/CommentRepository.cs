using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Infrastructure.Data.Entities;
using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;

namespace ServiceDeskSystem.Infrastructure.Data.Repository
{
    internal sealed class CommentRepository : TemplateRepository<Comment>
    {
        public CommentRepository(BugTrackerDbContext context)
            : base(context)
        {
        }

        protected override DbSet<Comment> DbSet => this.Context.Comments;

        public Comment? GetByIdWithAuthor(int id)
        {
            return this.Context.Comments
                .Include(c => c.Author)
                .FirstOrDefault(c => c.Id == id);
        }

        public async Task<Comment?> GetByIdWithAuthorAsync(int id)
        {
            return await this.Context.Comments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == id)
                .ConfigureAwait(false);
        }
    }
}
