using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Infrastructure.Data;
using DomainPerson = ServiceDeskSystem.Domain.Entities.Person;

namespace ServiceDeskSystem.Tests.Backend.Unit.Application.Services;

[TestFixture]
public class AuthServiceBogusTests
{
    private DbContextOptions<BugTrackerDbContext> _dbContextOptions = null!;
    private IDbContextFactory<BugTrackerDbContext> _contextFactory = null!;

    [SetUp]
    public void Setup()
    {
        _dbContextOptions = new DbContextOptionsBuilder<BugTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"BogusTestDb_{Guid.NewGuid()}")
            .Options;

        _contextFactory = new TestDbContextFactory(_dbContextOptions);
    }

    [Test]
    public async Task RegisterClientAsync_WithBogusData_ShouldCreateMultipleUsers()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        var testUsers = new[]
        {
            new { Username = "john_doe", Password = "SecurePass123", FirstName = "John", LastName = "Doe" },
            new { Username = "jane_smith", Password = "AnotherPass456", FirstName = "Jane", LastName = "Smith" },
            new { Username = "bob_wilson", Password = "BobsPassword789", FirstName = "Bob", LastName = "Wilson" }
        };

        // Act
        foreach (var testUser in testUsers)
        {
            var result = await service.RegisterClientAsync(
                testUser.Username,
                testUser.Password,
                testUser.FirstName,
                testUser.LastName,
                null);

            result.Success.Should().BeTrue();
        }

        // Assert
        await using var context = new BugTrackerDbContext(_dbContextOptions);
        var users = await context.Users.ToListAsync();
        users.Should().HaveCount(3);
        users.Select(u => u.Login).Should().Contain(testUsers.Select(t => t.Username));
    }

    [Test]
    public async Task LoginAsync_WithRandomBogusUsers_ShouldWorkForAllValidUsers()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        var faker = new Faker();
        var testData = Enumerable.Range(1, 5).Select(i => new
        {
            Username = faker.Internet.UserName(),
            Password = faker.Internet.Password(10),
            FirstName = faker.Name.FirstName(),
            LastName = faker.Name.LastName()
        }).ToList();

        // Act - Register all users
        foreach (var user in testData)
        {
            await service.RegisterClientAsync(
                user.Username,
                user.Password,
                user.FirstName,
                user.LastName,
                null);
        }

        // Assert - All should be able to login
        foreach (var user in testData)
        {
            var loginResult = await service.LoginAsync(user.Username, user.Password);
            loginResult.Success.Should().BeTrue($"User {user.Username} should be able to login");
        }
    }

    [Test]
    public async Task RegisterClientAsync_WithBogusEmail_ShouldStoreContactInfo()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await using var context = new BugTrackerDbContext(_dbContextOptions);
        
        context.ContactTypes.Add(new ContactType { Id = 1, Name = "Email" });
        await context.SaveChangesAsync();

        var faker = new Faker();
        var email = faker.Internet.Email();

        // Act
        var result = await service.RegisterClientAsync(
            faker.Internet.UserName(),
            faker.Internet.Password(12),
            faker.Name.FirstName(),
            faker.Name.LastName(),
            email);

        // Assert
        result.Success.Should().BeTrue();
        
        var contactInfo = await context.ContactInfos.FirstOrDefaultAsync();
        contactInfo.Should().NotBeNull();
        contactInfo!.Value.Should().Be(email);
    }

    [Test]
    [Repeat(10)] // Run this test 10 times with different random data
    public async Task PasswordHashing_WithBogusPasswords_ShouldProduceUniqueHashes()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        var faker = new Faker();
        var password = faker.Internet.Password(12);
        
        // Act - Register two users with same password
        await service.RegisterClientAsync(
            faker.Internet.UserName(),
            password,
            faker.Name.FirstName(),
            faker.Name.LastName(),
            null);

        await service.RegisterClientAsync(
            faker.Internet.UserName(),
            password,
            faker.Name.FirstName(),
            faker.Name.LastName(),
            null);

        // Assert - Hashes should be different due to unique salt
        await using var context = new BugTrackerDbContext(_dbContextOptions);
        var users = await context.Users.ToListAsync();
        users.Should().HaveCountGreaterThanOrEqualTo(2);

        var hash1 = users[^2].PasswordHash;
        var hash2 = users[^1].PasswordHash;
        hash1.Should().NotBe(hash2, "salts should make hashes unique even with same password");
    }

    [Test]
    public async Task RegisterClientAsync_WithInvalidBogusData_ShouldReturnAppropriateErrors()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        var faker = new Faker();

        // Act & Assert - Short password
        var shortPassResult = await service.RegisterClientAsync(
            faker.Internet.UserName(),
            "short",
            faker.Name.FirstName(),
            faker.Name.LastName(),
            null);
        shortPassResult.Success.Should().BeFalse();
        shortPassResult.ErrorMessage.Should().Contain("at least 8 characters");

        // Act & Assert - Empty first name
        var emptyNameResult = await service.RegisterClientAsync(
            faker.Internet.UserName(),
            faker.Internet.Password(12),
            "",
            faker.Name.LastName(),
            null);
        emptyNameResult.Success.Should().BeFalse();
        emptyNameResult.ErrorMessage.Should().Contain("First name and last name are required");
    }

    // Helper class for creating DbContext instances
    private class TestDbContextFactory : IDbContextFactory<BugTrackerDbContext>
    {
        private readonly DbContextOptions<BugTrackerDbContext> _options;

        public TestDbContextFactory(DbContextOptions<BugTrackerDbContext> options)
        {
            _options = options;
        }

        public BugTrackerDbContext CreateDbContext()
        {
            return new BugTrackerDbContext(_options);
        }

        public async Task<BugTrackerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new BugTrackerDbContext(_options));
        }
    }
}
