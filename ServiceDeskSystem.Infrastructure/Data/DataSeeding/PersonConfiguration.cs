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
            },
            new Person
            {
                Id = 6,
                FirstName = "Ірина",
                LastName = "Мельник",
                MiddleName = "Василівна",
                Bio = "Frontend-розробниця: Blazor, доступність і дизайн-системи.",
            },
            new Person
            {
                Id = 7,
                FirstName = "Дмитро",
                LastName = "Ткаченко",
                MiddleName = "Сергійович",
                Bio = "Mobile developer (Kotlin), відповідає за Mobile CRM.",
            },
            new Person
            {
                Id = 8,
                FirstName = "Софія",
                LastName = "Лисенко",
                MiddleName = "Андріївна",
                Bio = "Embedded-інженерка: прошивки POS-терміналів і контролерів замків.",
            },
            new Person
            {
                Id = 9,
                FirstName = "Олена",
                LastName = "Петренко",
                MiddleName = "Миколаївна",
                Bio = "Головна бухгалтерка, щодня працює з Бухгалтерія Pro.",
            },
            new Person
            {
                Id = 10,
                FirstName = "Kate",
                LastName = "Wilson",
                MiddleName = null,
                Bio = "Store operations lead for the e-commerce and POS rollout.",
            },
            new Person
            {
                Id = 11,
                FirstName = "Віктор",
                LastName = "Гриценко",
                MiddleName = "Іванович",
                Bio = "Начальник складу, користувач Warehouse Manager.",
            },
            new Person
            {
                Id = 12,
                FirstName = "Наталія",
                LastName = "Савчук",
                MiddleName = "Петрівна",
                Bio = "Колишня офіс-менеджерка; обліковий запис вимкнено.",
            });

        modelBuilder.Entity<ContactInfo>().HasData(
            Contacts(1, "admin@servicedesk.local", "+380 44 000 00 01")
                .Concat(Contacts(2, "o.kovalenko@servicedesk.local", "+380 67 210 45 18"))
                .Concat(Contacts(3, "m.shevchenko@client.ua", "+380 50 331 27 64"))
                .Concat(Contacts(4, "j.smith@client.com", "+1 415 555 0142"))
                .Concat(Contacts(5, "a.bondarenko@servicedesk.local", "+380 93 418 72 05"))
                .Concat(Contacts(6, "i.melnyk@servicedesk.local", "+380 67 554 09 31"))
                .Concat(Contacts(7, "d.tkachenko@servicedesk.local", "+380 63 702 15 88"))
                .Concat(Contacts(8, "s.lysenko@servicedesk.local", "+380 97 126 48 70"))
                .Concat(Contacts(9, "o.petrenko@client.ua", "+380 50 645 11 93"))
                .Concat(Contacts(10, "k.wilson@client.com", "+44 20 7946 0321"))
                .Concat(Contacts(11, "v.hrytsenko@client.ua", "+380 66 287 30 52"))
                .Concat(Contacts(12, "n.savchuk@client.ua", "+380 73 410 66 29"))
                .ToArray());
    }

    // Ids are 2 * person - 1 (email) and 2 * person (phone), so they stay stable as people are added.
    private static IEnumerable<ContactInfo> Contacts(int personId, string email, string phone) =>
    [
        new ContactInfo { Id = (2 * personId) - 1, PersonId = personId, ContactTypeId = 1, Value = email, IsPrimary = true },
        new ContactInfo { Id = 2 * personId, PersonId = personId, ContactTypeId = 2, Value = phone, IsPrimary = false },
    ];
}
