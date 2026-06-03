namespace ServiceDeskSystem.Domain.Interfaces;

public interface IRepositoryFacade : IAsyncDisposable
{
    ITicketRepository Tickets { get; }

    ICommentRepository Comments { get; }

    ITechStackRepository TechStacks { get; }

    IProductRepository Products { get; }

    IUserRepository Users { get; }

    IPersonRepository People { get; }

    IContactTypeRepository ContactTypes { get; }

    IContactInfoRepository ContactInfos { get; }

    IAuditLogRepository AuditLogs { get; }
    INotificationRepository Notifications { get; }

    IUnitOfWork UnitOfWork { get; }
}

