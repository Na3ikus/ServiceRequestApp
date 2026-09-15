using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding;

internal static class SlaPolicyConfiguration
{
    private static readonly DateTime SeedDate = new (2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SlaPolicy>().HasData(
            new SlaPolicy
            {
                Id = 1,
                Name = "Critical — default",
                Description = "Global fallback policy for critical incidents.",
                Priority = TicketPriority.Critical,
                ResponseMinutes = 60,
                ResolutionMinutes = 240,
                WarningThresholdPercent = 75,
                EscalateOnBreach = true,
                IsActive = true,
                CreatedAt = SeedDate,
            },
            new SlaPolicy
            {
                Id = 2,
                Name = "High — default",
                Description = "Global fallback policy for high priority tickets.",
                Priority = TicketPriority.High,
                ResponseMinutes = 240,
                ResolutionMinutes = 2880,
                WarningThresholdPercent = 80,
                EscalateOnBreach = true,
                IsActive = true,
                CreatedAt = SeedDate,
            },
            new SlaPolicy
            {
                Id = 3,
                Name = "Medium — default",
                Description = "Global fallback policy for medium priority tickets.",
                Priority = TicketPriority.Medium,
                ResponseMinutes = 480,
                ResolutionMinutes = 7200,
                WarningThresholdPercent = 80,
                EscalateOnBreach = false,
                IsActive = true,
                CreatedAt = SeedDate,
            },
            new SlaPolicy
            {
                Id = 4,
                Name = "Low — default",
                Description = "Global fallback policy for low priority tickets.",
                Priority = TicketPriority.Low,
                ResponseMinutes = 1440,
                ResolutionMinutes = 20160,
                WarningThresholdPercent = 90,
                EscalateOnBreach = false,
                IsActive = true,
                CreatedAt = SeedDate,
            });
    }
}
