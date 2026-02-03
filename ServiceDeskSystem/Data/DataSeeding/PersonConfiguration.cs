using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Data.DataSeeding;

internal static class PersonConfiguration
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>().HasData(
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
