using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Data.DataSeeding.Tables;

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasData(
            new Person
            {
                Id = 1,
                FirstName = "Admin",
                LastName = "Administrator",
                MiddleName = "System",
            },
            new Person
            {
                Id = 2,
                FirstName = "Іван",
                LastName = "Петренко",
                MiddleName = "Олександрович",
            },
            new Person
            {
                Id = 3,
                FirstName = "Марія",
                LastName = "Коваленко",
                MiddleName = "Іванівна",
            });
    }
}
