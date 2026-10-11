using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Components.Features.Tickets;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Tickets;

[TestFixture]
public class TicketActivityFeedTests
{
    private static readonly DateTime Created = new (2026, 10, 10, 9, 12, 0, DateTimeKind.Utc);

    private static Ticket NewTicket() => new ()
    {
        Id = 42,
        Title = "Report",
        Type = TicketType.Bug,
        Priority = TicketPriority.Medium,
        CreatedAt = Created,
        Author = new User { Login = "olena" },
        Comments =
        [
            new Comment { Id = 2, Message = "internal", IsInternal = true, CreatedAt = Created.AddHours(2), Author = new User { Login = "nazar" } },
            new Comment { Id = 1, Message = "public", CreatedAt = Created.AddMinutes(2), Author = new User { Login = "olena" } },
        ],
    };

    private static readonly TicketStatusHistory[] History =
    [
        new () { Id = 5, OldStatus = TicketStatus.InProgress, NewStatus = TicketStatus.Testing, ChangedAt = Created.AddHours(3), ChangedByUser = new User { Login = "nazar" } },
        new () { Id = 4, OldStatus = TicketStatus.Open, NewStatus = TicketStatus.InProgress, ChangedAt = Created.AddMinutes(53) },
    ];

    [Test]
    public void Merges_creation_status_changes_and_comments_by_time()
    {
        var feed = TicketActivityFeed.Build(NewTicket(), History, includeInternal: true);

        feed.Select(e => e.GetType().Name).Should().Equal(
            nameof(TicketCreatedActivity),
            nameof(TicketCommentActivity),
            nameof(TicketStatusActivity),
            nameof(TicketCommentActivity),
            nameof(TicketStatusActivity));
        feed.Select(e => e.At).Should().BeInAscendingOrder();
    }

    [Test]
    public void Created_event_carries_author_and_type()
    {
        var created = TicketActivityFeed.Build(NewTicket(), [], includeInternal: false)[0];

        created.Should().Be(new TicketCreatedActivity(Created, "olena", TicketType.Bug));
    }

    [Test]
    public void Internal_comments_are_left_out_for_users()
    {
        var feed = TicketActivityFeed.Build(NewTicket(), History, includeInternal: false);

        feed.OfType<TicketCommentActivity>().Select(c => c.Comment.Id).Should().Equal(1);
    }

    [Test]
    public void Backfilled_status_rows_have_no_actor()
    {
        var statuses = TicketActivityFeed.Build(NewTicket(), History, includeInternal: true).OfType<TicketStatusActivity>().ToList();

        statuses[0].Should().Be(new TicketStatusActivity(Created.AddMinutes(53), 4, null, TicketStatus.Open, TicketStatus.InProgress));
        statuses[1].ActorLogin.Should().Be("nazar");
    }

    [Test]
    public void At_equal_times_creation_comes_before_status_and_comments()
    {
        var ticket = NewTicket();
        ticket.Comments = [new Comment { Id = 9, Message = "same time", CreatedAt = Created }];
        TicketStatusHistory[] history = [new () { Id = 1, OldStatus = TicketStatus.New, NewStatus = TicketStatus.Open, ChangedAt = Created }];

        var feed = TicketActivityFeed.Build(ticket, history, includeInternal: false);

        feed.Select(e => e.GetType()).Should().Equal(typeof(TicketCreatedActivity), typeof(TicketStatusActivity), typeof(TicketCommentActivity));
    }
}
