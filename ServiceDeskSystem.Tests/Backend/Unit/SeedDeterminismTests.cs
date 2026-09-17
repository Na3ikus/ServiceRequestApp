using System;
using System.Linq;
using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using NUnit.Framework;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class SeedDeterminismTests
{
    private const int Pbkdf2Iterations = 100_000;

    private static BugTrackerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BugTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BugTrackerDbContext(options);
    }

    private static IReadOnlyEntityType SeededUserEntityType(BugTrackerDbContext context) =>
        context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(User))!;

    private static string[] SeededPasswordHashes(BugTrackerDbContext context) =>
        SeededUserEntityType(context)
            .GetSeedData()
            .OrderBy(d => (int)d[nameof(User.Id)]!)
            .Select(d => (string)d[nameof(User.PasswordHash)]!)
            .ToArray();

    [Test]
    public void SeededPasswordHashesAreStableAcrossModelBuilds()
    {
        using var first = CreateContext();
        using var second = CreateContext();

        var a = SeededPasswordHashes(first);
        var b = SeededPasswordHashes(second);

        a.Should().NotBeEmpty();
        a.Should().Equal(b);
    }

    [Test]
    public void SeededUsersUseDistinctSalts()
    {
        using var context = CreateContext();

        var salts = SeededPasswordHashes(context)
            .Select(h => h.Split(':')[0])
            .ToArray();

        salts.Should().OnlyHaveUniqueItems();
    }

    [TestCase(1, "admin123")]
    [TestCase(2, "dev123")]
    [TestCase(3, "client123")]
    [TestCase(4, "client123")]
    [TestCase(5, "dev123")]
    public void SeededPasswordHashVerifiesAgainstItsDocumentedPassword(int userId, string password)
    {
        using var context = CreateContext();

        var stored = SeededUserEntityType(context)
            .GetSeedData()
            .Single(d => (int)d[nameof(User.Id)]! == userId);

        var parts = ((string)stored[nameof(User.PasswordHash)]!).Split(':');
        parts.Should().HaveCount(2);

        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            32);

        actual.Should().Equal(expected);
    }
}
