using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Tests.Application.Services;

[TestFixture]
public class TicketServiceTests
{
    private DbContextOptions<BugTrackerDbContext> _dbContextOptions;
    private Mock<IDbContextFactory<BugTrackerDbContext>> _mockDbContextFactory;

    [SetUp]
    public void Setup()
    {
        _dbContextOptions = new DbContextOptionsBuilder<BugTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"BugTrackerDb_{Guid.NewGuid()}")
            .Options;

        _mockDbContextFactory = new Mock<IDbContextFactory<BugTrackerDbContext>>();
        _mockDbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new BugTrackerDbContext(_dbContextOptions));
        
        _mockDbContextFactory.Setup(f => f.CreateDbContext())
            .Returns(() => new BugTrackerDbContext(_dbContextOptions));
    }

    private BugTrackerDbContext CreateContext() => new BugTrackerDbContext(_dbContextOptions);

    [Test]
    public async Task CreateTicketAsync_GivenValidTicket_SetsCreatedAtAndStatusOpen()
    {
        // Arrange
        var service = new TicketService(_mockDbContextFactory.Object);
        var ticketToCreate = new Ticket
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = "High",
            ProductId = 1,
            AuthorId = 1
        };

        // Act
        var createdTicket = await service.CreateTicketAsync(ticketToCreate);

        // Assert
        createdTicket.Should().NotBeNull();
        createdTicket.Id.Should().BeGreaterThan(0);
        createdTicket.Status.Should().Be("Open");
        createdTicket.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        
        // Verify it was actually saved
        using var context = CreateContext();
        var savedTicket = await context.Tickets.FindAsync(createdTicket.Id);
        savedTicket.Should().NotBeNull();
        savedTicket!.Title.Should().Be("Test Ticket");
    }

    [Test]
    public async Task UpdateTicketStatusAsync_GivenExistingTicket_UpdatesStatus()
    {
        // Arrange
        var service = new TicketService(_mockDbContextFactory.Object);
        var ticket = new Ticket
        {
            Title = "Status Updatable Ticket",
            Status = "Open",
            Priority = "Medium"
        };
        
        using (var context = CreateContext())
        {
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await service.UpdateTicketStatusAsync(ticket.Id, "In Progress");

        // Assert
        result.Should().BeTrue();
        
        using (var context = CreateContext())
        {
            var updatedTicket = await context.Tickets.FindAsync(ticket.Id);
            updatedTicket!.Status.Should().Be("In Progress");
        }
    }

    [Test]
    public async Task AssignDeveloperAsync_GivenExistingTicket_AssignsDeveloper()
    {
        // Arrange
        var service = new TicketService(_mockDbContextFactory.Object);
        var ticket = new Ticket
        {
            Title = "Assignable Ticket",
            Status = "Open"
        };
        
        using (var context = CreateContext())
        {
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await service.AssignDeveloperAsync(ticket.Id, 2);

        // Assert
        result.Should().BeTrue();
        
        using (var context = CreateContext())
        {
            var updatedTicket = await context.Tickets.FindAsync(ticket.Id);
            updatedTicket!.DeveloperId.Should().Be(2);
        }
    }

    [Test]
    public async Task GetTicketByIdAsync_WhenTicketExists_ReturnsTicket()
    {
        // ... (existing code for GetTicketByIdAsync from line 125 onward)
        var service = new TicketService(_mockDbContextFactory.Object);
        var author = new User { Id = 1, Login = "author" };
        var product = new Product { Id = 1, Name = "Product1" };
        
        var ticket = new Ticket
        {
            Title = "Existing Ticket",
            Status = "Open",
            AuthorId = author.Id,
            Author = author,
            ProductId = product.Id,
            Product = product
        };
        
        using (var context = CreateContext())
        {
            context.Users.Add(author);
            context.Products.Add(product);
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await service.GetTicketByIdAsync(ticket.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(ticket.Id);
        result.Title.Should().Be("Existing Ticket");
    }

    [Test]
    public async Task GetAllTicketsAsync_WhenTicketsExist_ReturnsAllTickets()
    {
        // Arrange
        var service = new TicketService(_mockDbContextFactory.Object);
        var author1 = new User { Id = 5, Login = "author5" };
        var product1 = new Product { Id = 5, Name = "Product5" };
        using (var context = CreateContext())
        {
            context.Users.Add(author1);
            context.Products.Add(product1);
            context.Tickets.Add(new Ticket { Id = 10, Title = "T1", Status = "Open", AuthorId = author1.Id, ProductId = product1.Id });
            context.Tickets.Add(new Ticket { Id = 11, Title = "T2", Status = "Closed", AuthorId = author1.Id, ProductId = product1.Id });
            await context.SaveChangesAsync();
        }

        // Act
        var result = await service.GetAllTicketsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Should().Contain(t => t.Title == "T1");
        result.Should().Contain(t => t.Title == "T2");
    }

    [Test]
    public async Task DeleteTicketAsync_GivenExistingTicket_SuccessfullyDeletesIt()
    {
        // Arrange
        var service = new TicketService(_mockDbContextFactory.Object);
        var author2 = new User { Id = 6, Login = "author6" };
        var product2 = new Product { Id = 6, Name = "Product6" };
        var ticket = new Ticket { Id = 12, Title = "To Delete", Status = "Open", AuthorId = author2.Id, ProductId = product2.Id };
        
        using (var context = CreateContext())
        {
            context.Users.Add(author2);
            context.Products.Add(product2);
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await service.DeleteTicketAsync(ticket.Id);

        // Assert
        result.Should().BeTrue();
        using (var context = CreateContext())
        {
            var exists = await context.Tickets.AnyAsync(t => t.Id == ticket.Id);
            exists.Should().BeFalse();
        }
    }

    [Test]
    public async Task AddCommentAsync_GivenValidComment_SavesAndReturnsComment()
    {
        // Arrange
        var service = new TicketService(_mockDbContextFactory.Object);
        var ticket = new Ticket { Title = "Ticket for comment", Status = "Open" };
        var author = new User { Id = 3, Login = "commenter" };
        
        using (var context = CreateContext())
        {
            context.Users.Add(author);
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
        }

        var comment = new Comment
        {
            TicketId = ticket.Id,
            AuthorId = author.Id,
            Message = "New Comment"
        };

        // Act
        var result = await service.AddCommentAsync(comment);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Message.Should().Be("New Comment");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        using (var context = CreateContext())
        {
            var savedComment = await context.Comments.FindAsync(result.Id);
            savedComment.Should().NotBeNull();
        }
    }
}
