using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding;

public sealed class DbInitializer(BugTrackerDbContext context)
{
    private readonly BugTrackerDbContext _context = context;

    public async Task InitializeAsync()
    {
        if (_context.Database.IsRelational())
        {
            await _context.Database.MigrateAsync().ConfigureAwait(false);
        }
        else
        {
            await _context.Database.EnsureCreatedAsync().ConfigureAwait(false);
        }

        await SeedDefaultTagsAsync().ConfigureAwait(false);
    }

    private async Task SeedDefaultTagsAsync()
    {
        if (!await _context.Tags.AnyAsync().ConfigureAwait(false))
        {
            var adminUser = await _context.Users.FirstOrDefaultAsync().ConfigureAwait(false);
            int adminId = adminUser?.Id ?? 1;

            var defaultTags = new List<Tag>
            {
                new() { Name = "Bug", Color = "#ef4444", Description = "Bug / defect in system", CreatedById = adminId },
                new() { Name = "Feature", Color = "#3b82f6", Description = "New feature or enhancement", CreatedById = adminId },
                new() { Name = "UI/UX", Color = "#8b5cf6", Description = "User interface and styling", CreatedById = adminId },
                new() { Name = "Backend", Color = "#10b981", Description = "API, business logic & database", CreatedById = adminId },
                new() { Name = "Urgent", Color = "#f97316", Description = "High priority critical issue", CreatedById = adminId },
                new() { Name = "Documentation", Color = "#06b6d4", Description = "Docs and specs", CreatedById = adminId },
                new() { Name = "Optimization", Color = "#eab308", Description = "Performance and refactoring", CreatedById = adminId }
            };

            await _context.Tags.AddRangeAsync(defaultTags).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}

