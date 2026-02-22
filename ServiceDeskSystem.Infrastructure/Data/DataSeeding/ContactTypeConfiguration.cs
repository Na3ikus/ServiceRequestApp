using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding;

internal static class ContactTypeConfiguration
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContactType>().HasData(
            new ContactType { Id = 1, Name = "Email" },
            new ContactType { Id = 2, Name = "Phone" },
            new ContactType { Id = 3, Name = "Telegram" });
    }
}
