using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding;

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
                Bio = "Senior .NET Developer з досвідом понад 5 років.",
            },
            new Person
            {
                Id = 3,
                FirstName = "Марія",
                LastName = "Шевченко",
                MiddleName = "Іванівна",
                Bio = "Менеджер з персоналу (HR). Завжди на зв'язку для вирішення питань співробітників.",
            },
            new Person
            {
                Id = 4,
                FirstName = "John",
                LastName = "Smith",
                MiddleName = null,
                Bio = "Project Manager overseeing the new internal portal development.",
            },
            new Person
            {
                Id = 5,
                FirstName = "Андрій",
                LastName = "Бондаренко",
                MiddleName = "Олегович",
                Bio = "QA Engineer, спеціаліст з автоматизованого тестування.",
            },
            new Person
            {
                Id = 1,
                FirstName = "System",
                LastName = "Administrator",
                MiddleName = null,
                Bio = "Головний адміністратор системи Service Desk.",
            });
    }
}
