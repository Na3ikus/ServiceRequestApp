namespace ServiceDeskSystem.Domain.Interfaces;

public interface IRepositoryFacade : IAsyncDisposable
{
    ITicketRepository Tickets { get; }

    ICommentRepository Comments { get; }

    ITechStackRepository TechStacks { get; }

    IProductRepository Products { get; }

    IUserRepository Users { get; }

    Task<int> SaveChangesAsync();
}
