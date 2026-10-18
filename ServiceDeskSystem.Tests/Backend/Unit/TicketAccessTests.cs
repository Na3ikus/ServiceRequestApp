using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class TicketAccessTests
{
    private static readonly Ticket Ticket = new () { Id = 5, AuthorId = 10, DeveloperId = 7 };

    [TestCase(UserRole.Admin, 1)]
    [TestCase(UserRole.Developer, 7)]
    [TestCase(UserRole.Developer, 99)]
    [TestCase(UserRole.User, 10)]
    public void Staff_and_the_author_can_view(UserRole role, int userId)
    {
        TicketAccess.CanView(Ticket, new User { Id = userId, Role = role }).Should().BeTrue();
    }

    [Test]
    public void Another_user_cannot_view()
    {
        TicketAccess.CanView(Ticket, new User { Id = 11, Role = UserRole.User }).Should().BeFalse();
    }

    [Test]
    public void Nobody_signed_in_cannot_view()
    {
        TicketAccess.CanView(Ticket, null).Should().BeFalse();
        TicketAccess.CanView(Ticket, null, null).Should().BeFalse();
        TicketAccess.CanView(Ticket, 10, null).Should().BeFalse("a token without a role is not the author's session");
    }
}
