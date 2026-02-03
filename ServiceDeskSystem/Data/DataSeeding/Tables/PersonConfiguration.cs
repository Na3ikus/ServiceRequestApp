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
                Id = 2,
                FirstName = "Олександр",
                LastName = "Коваленко",
                MiddleName = "Петрович",
            },
            new Person
            {
                Id = 3,
                FirstName = "Марія",
                LastName = "Шевченко",
                MiddleName = "Іванівна",
            },
            new Person
            {
                Id = 4,
                FirstName = "John",
                LastName = "Smith",
                MiddleName = null,
            },
            new Person
            {
                Id = 5,
                FirstName = "Андрій",
                LastName = "Бондаренко",
                MiddleName = "Олегович",
            },
            new Person
            {
                Id = 1,
                FirstName = "System",
                LastName = "Administrator",
                MiddleName = null,
            });
    }
}
