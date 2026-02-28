using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Tests.Domain.Entities;

[TestFixture]
public class TicketTests
{
    [Test]
    public void Ticket_WhenInstantiated_ShouldHaveEmptyStringsAndEmptyCollections()
    {
        // Act
        var ticket = new Ticket();

        // Assert
        ticket.Title.Should().BeEmpty();
        ticket.Description.Should().BeEmpty();
        ticket.StepsToReproduce.Should().BeEmpty();
        ticket.Priority.Should().BeEmpty();
        ticket.Status.Should().BeEmpty();
        ticket.AffectedVersion.Should().BeEmpty();
        ticket.Environment.Should().BeEmpty();
        
        ticket.Comments.Should().NotBeNull();
        ticket.Comments.Should().BeEmpty();
        
        ticket.Attachments.Should().NotBeNull();
        ticket.Attachments.Should().BeEmpty();
    }
}
