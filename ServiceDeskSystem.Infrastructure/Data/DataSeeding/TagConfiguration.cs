using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding;

internal static class TagConfiguration
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Bug", Color = "#EF4444" },
            new Tag { Id = 2, Name = "UI/UX", Color = "#8B5CF6" },
            new Tag { Id = 3, Name = "Backend", Color = "#3B82F6" },
            new Tag { Id = 4, Name = "Urgent", Color = "#F59E0B" },
            new Tag { Id = 5, Name = "Feature", Color = "#10B981" },
            new Tag { Id = 6, Name = "Security", Color = "#EC4899" },
            new Tag { Id = 7, Name = "Performance", Color = "#EAB308", Description = "Speed, memory and load", CreatedById = 1 },
            new Tag { Id = 8, Name = "Integration", Color = "#06B6D4", Description = "Payment gateways, banks, ERP and other external systems", CreatedById = 1 },
            new Tag { Id = 9, Name = "Documentation", Color = "#64748B", Description = "Guides, specs and how-to answers", CreatedById = 1 });
    }
}
