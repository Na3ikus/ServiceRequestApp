using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Data.DataSeeding;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasData(
            new Product
            {
                Id = 3,
                Name = "HR Portal",
                Description = "Корпоративний портал для управління персоналом",
                CurrentVersion = "1.8.3",
                TechStackId = 2,
            },
            new Product
            {
                Id = 4,
                Name = "E-Commerce Platform",
                Description = "Платформа для онлайн-продажів з інтеграцією платіжних систем",
                CurrentVersion = "4.1.0",
                TechStackId = 2,
            },
            new Product
            {
                Id = 5,
                Name = "Mobile CRM",
                Description = "Мобільний додаток для роботи з клієнтською базою",
                CurrentVersion = "2.0.5",
                TechStackId = 3,
            },
            new Product
            {
                Id = 6,
                Name = "POS Terminal v2",
                Description = "Прошивка для касових терміналів",
                CurrentVersion = "1.4.2",
                TechStackId = 4,
            },
            new Product
            {
                Id = 7,
                Name = "Smart Lock Controller",
                Description = "Контролер системи контролю доступу",
                CurrentVersion = "2.1.0",
                TechStackId = 4,
            },
            new Product
            {
                Id = 1,
                Name = "Бухгалтерія Pro",
                Description = "Програма для ведення бухгалтерського обліку підприємства",
                CurrentVersion = "3.2.1",
                TechStackId = 1,
            },
            new Product
            {
                Id = 2,
                Name = "Warehouse Manager",
                Description = "Система управління складом та інвентаризацією",
                CurrentVersion = "2.5.0",
                TechStackId = 1,
            },
            new Product
            {
                Id = 8,
                Name = "Office Router Pro",
                Description = "Корпоративний маршрутизатор з підтримкою VPN",
                CurrentVersion = "5.0.1",
                TechStackId = 6,
            });
    }
}
