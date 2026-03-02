using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Tests.Backend.Unit.Domain.Entities;

[TestFixture]
public class UserTests
{
    [Test]
    public void User_WhenInstantiated_ShouldHaveDefaultValues()
    {
        // Act
        var user = new User();

        // Assert
        user.Id.Should().Be(0);
        user.Login.Should().BeEmpty();
        user.PasswordHash.Should().BeEmpty();
        user.Role.Should().BeEmpty();
        user.IsActive.Should().BeTrue(); // Default is true
    }

    [Test]
    public void User_ShouldAllowSettingProperties()
    {
        // Arrange
        var user = new User();

        // Act
        user.Login = "testuser";
        user.PasswordHash = "hashedpassword";
        user.Role = "Admin";
        user.IsActive = true;
        user.PersonId = 1;

        // Assert
        user.Login.Should().Be("testuser");
        user.PasswordHash.Should().Be("hashedpassword");
        user.Role.Should().Be("Admin");
        user.IsActive.Should().BeTrue();
        user.PersonId.Should().Be(1);
    }
}

[TestFixture]
public class PersonTests
{
    [Test]
    public void Person_WhenInstantiated_ShouldHaveEmptyCollections()
    {
        // Act
        var person = new Person();

        // Assert
        person.ContactInfos.Should().NotBeNull();
        person.ContactInfos.Should().BeEmpty();
    }

    [Test]
    public void Person_ShouldAllowAddingContactInfo()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        person.ContactInfos.Add(new ContactInfo
        {
            ContactTypeId = 1,
            Value = "john@example.com"
        });

        // Assert
        person.ContactInfos.Should().HaveCount(1);
        person.ContactInfos.First().Value.Should().Be("john@example.com");
    }
}

[TestFixture]
public class ProductTests
{
    [Test]
    public void Product_WhenInstantiated_ShouldHaveEmptyCollections()
    {
        // Act
        var product = new Product();

        // Assert
        product.Name.Should().BeEmpty();
        product.Description.Should().BeEmpty();
        product.CurrentVersion.Should().BeEmpty();
        product.Tickets.Should().NotBeNull();
        product.Tickets.Should().BeEmpty();
    }

    [Test]
    public void Product_ShouldAllowSettingProperties()
    {
        // Act
        var product = new Product
        {
            Name = "Test Product",
            Description = "A test product",
            CurrentVersion = "1.0.0",
            TechStackId = 1
        };

        // Assert
        product.Name.Should().Be("Test Product");
        product.Description.Should().Be("A test product");
        product.CurrentVersion.Should().Be("1.0.0");
        product.TechStackId.Should().Be(1);
    }
}

[TestFixture]
public class CommentTests
{
    [Test]
    public void Comment_WhenInstantiated_ShouldHaveDefaultValues()
    {
        // Act
        var comment = new Comment();

        // Assert
        comment.Message.Should().BeEmpty();
        comment.CreatedAt.Should().Be(default);
        comment.AuthorId.Should().Be(0);
        comment.TicketId.Should().Be(0);
        comment.IsInternal.Should().BeFalse();
    }

    [Test]
    public void Comment_ShouldAllowSettingProperties()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;

        // Act
        var comment = new Comment
        {
            Message = "Test comment",
            CreatedAt = createdAt,
            AuthorId = 1,
            TicketId = 5,
            IsInternal = false
        };

        // Assert
        comment.Message.Should().Be("Test comment");
        comment.CreatedAt.Should().Be(createdAt);
        comment.AuthorId.Should().Be(1);
        comment.TicketId.Should().Be(5);
        comment.IsInternal.Should().BeFalse();
    }
}
