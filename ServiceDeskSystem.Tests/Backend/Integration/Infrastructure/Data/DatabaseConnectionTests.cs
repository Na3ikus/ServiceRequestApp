using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Tests.Backend.Integration.Infrastructure.Data;

[TestFixture]
public class DatabaseConnectionTests
{
    private DbContextOptions<BugTrackerDbContext> _dbContextOptions;

    [SetUp]
    public void Setup()
    {
        _dbContextOptions = new DbContextOptionsBuilder<BugTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"ConnectionTestDb_{Guid.NewGuid()}")
            .Options;
    }

    [Test]
    public async Task BugTrackerDbContext_WhenInstantiated_CanConnectSuccessfully()
    {
        // Act
        await using var context = new BugTrackerDbContext(_dbContextOptions);
        var canConnect = await context.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue("The database connection should be successful.");
    }
}
