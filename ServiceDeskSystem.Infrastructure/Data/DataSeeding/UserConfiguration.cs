using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding;

internal static class UserConfiguration
{
    private const int Pbkdf2Iterations = 100_000;

    private static string ComputeSecureHash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            32);

        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Login = "admin",
                PasswordHash = ComputeSecureHash("admin123"),
                Role = "Admin",
                PersonId = 1,
                IsActive = true,
            },
            new User
            {
                Id = 2,
                Login = "o.kovalenko",
                PasswordHash = ComputeSecureHash("dev123"),
                Role = "Developer",
                PersonId = 2,
                IsActive = true,
            },
            new User
            {
                Id = 3,
                Login = "m.shevchenko",
                PasswordHash = ComputeSecureHash("client123"),
                Role = "User",
                PersonId = 3,
                IsActive = true,
            },
            new User
            {
                Id = 4,
                Login = "j.smith",
                PasswordHash = ComputeSecureHash("client123"),
                Role = "User",
                PersonId = 4,
                IsActive = true,
            },
            new User
            {
                Id = 5,
                Login = "a.bondarenko",
                PasswordHash = ComputeSecureHash("dev123"),
                Role = "Developer",
                PersonId = 5,
                IsActive = true,
            });
    }
}

