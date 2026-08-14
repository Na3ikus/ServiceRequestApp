using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Domain.Interfaces.Repositories;

public interface IAttachmentRepository : IRepository<Attachment>
{
    Task<IEnumerable<Attachment>> GetByTicketIdAsync(int ticketId);
}
