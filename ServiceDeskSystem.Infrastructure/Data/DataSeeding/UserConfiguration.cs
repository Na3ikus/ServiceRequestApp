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
    private const string MelnykPasswordHash = "U3ZjRGVza1NlZWRTYWx0Ng==:hcoI8IiAEtldbK7YNWXagdJb6h5UwIK3BjDV9rIpQwg=";
    private const string TkachenkoPasswordHash = "U3ZjRGVza1NlZWRTYWx0Nw==:SV1oOp7MMGzWb1K/+BdZ52EfbO0X0jQOAI0WSCc0dUk=";
    private const string LysenkoPasswordHash = "U3ZjRGVza1NlZWRTYWx0OA==:nf2tdiHNorD4PO+dxpRQfb7VYIZNUoeHtbsYX99VK5I=";
    private const string PetrenkoPasswordHash = "U3ZjRGVza1NlZWRTYWx0OQ==:mjKwqNqTOVttaW6+4Or/eh3ms4VqpZPc7iEqQSElwNY=";
    private const string WilsonPasswordHash = "U3ZjRGVza1NlZWRTYWx0QQ==:eFGwufuXgHOfB0LiKECswrPgi7oMkp6gUEq4Godj/6Q=";
    private const string HrytsenkoPasswordHash = "U3ZjRGVza1NlZWRTYWx0Qg==:B6FENngup9HJCakIq4LVrT/GQcA1WA+pwOG/t6SrYhs=";
    private const string SavchukPasswordHash = "U3ZjRGVza1NlZWRTYWx0Qw==:8oD33HADYdlGZt/JOJ/JXrd0CBel2/x6etfzavZb1JQ=";

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
            },
            new User { Id = 6, Login = "i.melnyk", PasswordHash = MelnykPasswordHash, Role = UserRole.Developer, PersonId = 6, IsActive = true },
            new User { Id = 7, Login = "d.tkachenko", PasswordHash = TkachenkoPasswordHash, Role = UserRole.Developer, PersonId = 7, IsActive = true },
            new User { Id = 8, Login = "s.lysenko", PasswordHash = LysenkoPasswordHash, Role = UserRole.Developer, PersonId = 8, IsActive = true },
            new User { Id = 9, Login = "o.petrenko", PasswordHash = PetrenkoPasswordHash, Role = UserRole.User, PersonId = 9, IsActive = true },
            new User { Id = 10, Login = "k.wilson", PasswordHash = WilsonPasswordHash, Role = UserRole.User, PersonId = 10, IsActive = true },
            new User { Id = 11, Login = "v.hrytsenko", PasswordHash = HrytsenkoPasswordHash, Role = UserRole.User, PersonId = 11, IsActive = true },
            new User { Id = 12, Login = "n.savchuk", PasswordHash = SavchukPasswordHash, Role = UserRole.User, PersonId = 12, IsActive = false });
    }
}
