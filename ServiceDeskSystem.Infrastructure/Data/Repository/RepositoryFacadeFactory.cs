using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Infrastructure.Data.Repository;

public sealed class RepositoryFacadeFactory(IDbContextFactory<BugTrackerDbContext> contextFactory) : IRepositoryFacadeFactory
{
    public IRepositoryFacade Create()
    {
        return new RepositoryFacade(contextFactory);
    }
}
