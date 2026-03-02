using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Tests.Backend.Unit.Application.Services;

[TestFixture]
public class AuthServiceTests
{
    private DbContextOptions<BugTrackerDbContext> _dbContextOptions = null!;
    private IDbContextFactory<BugTrackerDbContext> _contextFactory = null!;

    [SetUp]
    public void Setup()
    {
        _dbContextOptions = new DbContextOptionsBuilder<BugTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"AuthTestDb_{Guid.NewGuid()}")
            .Options;

        _contextFactory = new TestDbContextFactory(_dbContextOptions);
    }

    private async Task<User> SeedTestUserAsync(string login, string password, bool isActive = true)
    {
        await using var context = new BugTrackerDbContext(_dbContextOptions);
        
        var person = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        context.People.Add(person);
        await context.SaveChangesAsync();

        // Use the same hashing logic as AuthService
        var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        var hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100_000,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32);
        
        var user = new User
        {
            Login = login,
            PasswordHash = Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash),
            Role = "User",
            PersonId = person.Id,
            IsActive = isActive
        };
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        return user;
    }

    #region LoginAsync Tests

    [Test]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await SeedTestUserAsync("testuser", "password123");

        // Act
        var result = await service.LoginAsync("testuser", "password123");

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        service.IsAuthenticated.Should().BeTrue();
        service.CurrentUser.Should().NotBeNull();
        service.CurrentUser!.Login.Should().Be("testuser");
    }

    [Test]
    public async Task LoginAsync_WithInvalidUsername_ShouldReturnError()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await SeedTestUserAsync("testuser", "password123");

        // Act
        var result = await service.LoginAsync("wronguser", "password123");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid username or password.");
        service.IsAuthenticated.Should().BeFalse();
    }

    [Test]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnError()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await SeedTestUserAsync("testuser", "password123");

        // Act
        var result = await service.LoginAsync("testuser", "wrongpassword");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid username or password.");
        service.IsAuthenticated.Should().BeFalse();
    }

    [Test]
    public async Task LoginAsync_WithEmptyUsername_ShouldReturnError()
    {
        // Arrange
        var service = new AuthService(_contextFactory);

        // Act
        var result = await service.LoginAsync("", "password123");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Username and password are required.");
    }

    [Test]
    public async Task LoginAsync_WithEmptyPassword_ShouldReturnError()
    {
        // Arrange
        var service = new AuthService(_contextFactory);

        // Act
        var result = await service.LoginAsync("testuser", "");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Username and password are required.");
    }

    [Test]
    public async Task LoginAsync_WithInactiveUser_ShouldReturnError()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await SeedTestUserAsync("inactiveuser", "password123", isActive: false);

        // Act
        var result = await service.LoginAsync("inactiveuser", "password123");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Account is deactivated. Please contact administrator.");
        service.IsAuthenticated.Should().BeFalse();
    }

    [Test]
    public async Task LoginAsync_ShouldRaiseAuthStateChangedEvent()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await SeedTestUserAsync("testuser", "password123");
        var eventRaised = false;
        service.AuthStateChanged += (sender, args) => eventRaised = true;

        // Act
        await service.LoginAsync("testuser", "password123");

        // Assert
        eventRaised.Should().BeTrue();
    }

    #endregion

    #region RegisterClientAsync Tests

    [Test]
    public async Task RegisterClientAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await using var context = new BugTrackerDbContext(_dbContextOptions);
        
        // Create Email contact type
        context.ContactTypes.Add(new ContactType { Id = 1, Name = "Email" });
        await context.SaveChangesAsync();

        // Act
        var result = await service.RegisterClientAsync(
            "newuser",
            "password123",
            "John",
            "Doe",
            "john@example.com");

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();

        // Verify user was created in DB
        var user = await context.Users.FirstOrDefaultAsync(u => u.Login == "newuser");
        user.Should().NotBeNull();
        user!.Role.Should().Be("User");
        user.IsActive.Should().BeTrue();
        user.PasswordHash.Should().Contain(":"); // PBKDF2 format

        // Verify person was created
        var person = await context.People.FindAsync(user.PersonId);
        person.Should().NotBeNull();
        person!.FirstName.Should().Be("John");
        person.LastName.Should().Be("Doe");
    }

    [Test]
    public async Task RegisterClientAsync_WithShortPassword_ShouldReturnError()
    {
        // Arrange
        var service = new AuthService(_contextFactory);

        // Act
        var result = await service.RegisterClientAsync(
            "newuser",
            "short",
            "John",
            "Doe",
            null);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("at least 8 characters");
    }

    [Test]
    public async Task RegisterClientAsync_WithExistingUsername_ShouldReturnError()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await SeedTestUserAsync("existinguser", "password123");

        // Act
        var result = await service.RegisterClientAsync(
            "existinguser",
            "newpassword123",
            "John",
            "Doe",
            null);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Username already exists.");
    }

    [Test]
    public async Task RegisterClientAsync_WithEmptyFirstName_ShouldReturnError()
    {
        // Arrange
        var service = new AuthService(_contextFactory);

        // Act
        var result = await service.RegisterClientAsync(
            "newuser",
            "password123",
            "",
            "Doe",
            null);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("First name and last name are required.");
    }

    [Test]
    public async Task RegisterClientAsync_WithExistingEmail_ShouldReturnError()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await using var context = new BugTrackerDbContext(_dbContextOptions);
        
        var emailType = new ContactType { Id = 1, Name = "Email" };
        context.ContactTypes.Add(emailType);
        
        var person = new Person { FirstName = "Existing", LastName = "User" };
        context.People.Add(person);
        await context.SaveChangesAsync();

        var contact = new ContactInfo
        {
            ContactTypeId = emailType.Id,
            PersonId = person.Id,
            Value = "existing@example.com"
        };
        context.ContactInfos.Add(contact);
        await context.SaveChangesAsync();

        // Act
        var result = await service.RegisterClientAsync(
            "newuser",
            "password123",
            "John",
            "Doe",
            "existing@example.com");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Email address is already registered.");
    }

    #endregion

    #region LogoutAsync Tests

    [Test]
    public async Task LogoutAsync_WhenAuthenticated_ShouldClearCurrentUser()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await SeedTestUserAsync("testuser", "password123");
        await service.LoginAsync("testuser", "password123");

        // Act
        await service.LogoutAsync();

        // Assert
        service.IsAuthenticated.Should().BeFalse();
        service.CurrentUser.Should().BeNull();
    }

    [Test]
    public async Task LogoutAsync_ShouldRaiseAuthStateChangedEvent()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await SeedTestUserAsync("testuser", "password123");
        await service.LoginAsync("testuser", "password123");
        
        var eventRaised = false;
        service.AuthStateChanged += (sender, args) => eventRaised = true;

        // Act
        await service.LogoutAsync();

        // Assert
        eventRaised.Should().BeTrue();
    }

    #endregion

    #region Password Security Tests

    [Test]
    public async Task RegisterAndLogin_ShouldUsePbkdf2Hashing()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await using var context = new BugTrackerDbContext(_dbContextOptions);

        // Act - Register
        await service.RegisterClientAsync("secureuser", "MySecureP@ss123", "Secure", "User", null);

        // Assert - Check hash format
        var user = await context.Users.FirstOrDefaultAsync(u => u.Login == "secureuser");
        user.Should().NotBeNull();
        user!.PasswordHash.Should().Contain(":");
        user.PasswordHash.Split(':').Should().HaveCount(2);

        // Act - Login with correct password
        var loginResult = await service.LoginAsync("secureuser", "MySecureP@ss123");
        loginResult.Success.Should().BeTrue();
    }

    [Test]
    public async Task RegisterClientAsync_WithSamePassword_ShouldProduceDifferentHashes()
    {
        // Arrange
        var service = new AuthService(_contextFactory);
        await using var context = new BugTrackerDbContext(_dbContextOptions);

        // Act - Register two users with same password
        await service.RegisterClientAsync("user1", "SamePassword123", "User", "One", null);
        await service.RegisterClientAsync("user2", "SamePassword123", "User", "Two", null);

        // Assert - Hashes should be different due to unique salt
        var user1 = await context.Users.FirstOrDefaultAsync(u => u.Login == "user1");
        var user2 = await context.Users.FirstOrDefaultAsync(u => u.Login == "user2");
        
        user1!.PasswordHash.Should().NotBe(user2!.PasswordHash);
    }

    #endregion

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
