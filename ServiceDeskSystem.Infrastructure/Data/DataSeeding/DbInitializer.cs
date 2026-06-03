using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding;

public sealed class DbInitializer
{
    private readonly BugTrackerDbContext _context;

    public DbInitializer(BugTrackerDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }
}
