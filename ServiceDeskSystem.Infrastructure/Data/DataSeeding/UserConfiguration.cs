using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding;

/// <summary>
/// Seeds the demo accounts. Password hashes are fixed literals rather than computed at
/// model-build time: a freshly salted hash on every build makes the seed non-deterministic,
/// which makes every new EF migration rewrite these rows.
/// </summary>
internal static class UserConfiguration
{
    private const string AdminPasswordHash = "U3ZjRGVza1NlZWRTYWx0MQ==:orhXRQyasnY0tbIjWT972XgUcYbQYFJOZKf3kiGWZqc=";
    private const string KovalenkoPasswordHash = "U3ZjRGVza1NlZWRTYWx0Mg==:FkNHdz+zH4OkjeLqfT7fVPpxlPIko9BPjX0oXp5SQiQ=";
    private const string ShevchenkoPasswordHash = "U3ZjRGVza1NlZWRTYWx0Mw==:NPGoKnpCuSvcy5UOjkFBuUsfPWfY7Wy9n8HEZZZBA30=";
    private const string SmithPasswordHash = "U3ZjRGVza1NlZWRTYWx0NA==:zggZqFDVV5pvh+rK54pZ0zRivz8AGkR4u4NeVmEdlr4=";
    private const string BondarenkoPasswordHash = "U3ZjRGVza1NlZWRTYWx0NQ==:aluImitgI/5MSw+YERyrtv48OrALZ/MfTegJlKLJfYo=";

    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Login = "admin",
                PasswordHash = AdminPasswordHash,
                Role = UserRole.Admin,
                PersonId = 1,
                IsActive = true,
            },
            new User
            {
                Id = 2,
                Login = "o.kovalenko",
                PasswordHash = KovalenkoPasswordHash,
                Role = UserRole.Developer,
                PersonId = 2,
                IsActive = true,
            },
            new User
            {
                Id = 3,
                Login = "m.shevchenko",
                PasswordHash = ShevchenkoPasswordHash,
                Role = UserRole.User,
                PersonId = 3,
                IsActive = true,
            },
            new User
            {
                Id = 4,
                Login = "j.smith",
                PasswordHash = SmithPasswordHash,
                Role = UserRole.User,
                PersonId = 4,
                IsActive = true,
            },
            new User
            {
                Id = 5,
                Login = "a.bondarenko",
                PasswordHash = BondarenkoPasswordHash,
                Role = UserRole.Developer,
                PersonId = 5,
                IsActive = true,
            });
    }
}
