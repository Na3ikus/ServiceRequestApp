using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Data.DataSeeding;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasData(
            new User
            {
                Id = 1,
                Login = "admin",
                PasswordHash = "admin123",
                Role = "Admin",
                PersonId = 1,
                IsActive = true,
            },
            new User
            {
                Id = 2,
                Login = "o.kovalenko",
                PasswordHash = "dev123",
                Role = "Developer",
                PersonId = 2,
                IsActive = true,
            },
            new User
            {
                Id = 3,
                Login = "m.shevchenko",
                PasswordHash = "client123",
                Role = "User",
                PersonId = 3,
                IsActive = true,
            },
            new User
            {
                Id = 4,
                Login = "j.smith",
                PasswordHash = "client123",
                Role = "User",
                PersonId = 4,
                IsActive = true,
            },
            new User
            {
                Id = 5,
                Login = "a.bondarenko",
                PasswordHash = "dev123",
                Role = "Developer",
                PersonId = 5,
                IsActive = true,
            });
    }
}
