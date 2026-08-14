using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces.Repositories;
using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;

namespace ServiceDeskSystem.Infrastructure.Data.Repository;

public sealed class AttachmentRepository : TemplateRepository<Attachment>, IAttachmentRepository
{
    public AttachmentRepository(BugTrackerDbContext context)
        : base(context)
    {
    }

    protected override DbSet<Attachment> DbSet => this.Context.Attachments;

    public async Task<IEnumerable<Attachment>> GetByTicketIdAsync(int ticketId)
    {
        return await this.DbSet
            .Include(a => a.UploadedBy)
            .ThenInclude(u => u!.Person)
            .Where(a => a.TicketId == ticketId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
