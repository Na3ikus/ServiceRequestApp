using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data.Entities;
using ServiceDeskSystem.Data.Repository.Templates;

namespace ServiceDeskSystem.Data.Repository
{
    internal sealed class CommentRepository : TemplateRepository<Comment>
    {
        public CommentRepository(BugTrackerDbContext context)
            : base(context)
        {
        }

        protected override DbSet<Comment> DbSet => this.Context.Comments;

        public Comment? GetByIdWithAuthor(long id)
        {
            return this.Context.Comments
                .Include(c => c.Author)
                .FirstOrDefault(c => c.Id == id);
        }

        public async Task<Comment?> GetByIdWithAuthorAsync(long id)
        {
            return await this.Context.Comments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == id)
                .ConfigureAwait(false);
        }
    }
}
