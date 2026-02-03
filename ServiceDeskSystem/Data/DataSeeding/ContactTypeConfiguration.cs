using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Data.DataSeeding;

internal sealed class ContactTypeConfiguration : IEntityTypeConfiguration<ContactType>
{
    public void Configure(EntityTypeBuilder<ContactType> builder)
    {
        builder.HasData(
            new ContactType { Id = 1, Name = "Email" },
            new ContactType { Id = 2, Name = "Phone" },
            new ContactType { Id = 3, Name = "Telegram" });
    }
}
