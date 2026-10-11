using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Attachments;
using ServiceDeskSystem.Application.Services.Comments;
using ServiceDeskSystem.Application.Services.Tags;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.WorkLogs;
using ServiceDeskSystem.Components.Features.Tickets;
using ServiceDeskSystem.Components.Pages.Tickets;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Tickets;

[TestFixture]
public class TicketDetailsPageTests
{
    private const int AuthorId = 10;
    private const int DeveloperId = 7;

    private ComponentTestHost _host;
    private Mock<ITicketService> _tickets;
    private Mock<ITagService> _tags;
    private Ticket _ticket;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();
        _host.TimeZone.Set("Europe/Kyiv");
        _host.Auth.SetupGet(a => a.IsAuthenticated).Returns(true);

        _ticket = new Ticket
        {
            Id = 42,
            Title = "Monthly report is empty",
            Description = "<p>Blank screen</p>",
            Type = TicketType.Bug,
            Priority = TicketPriority.Medium,
            Status = TicketStatus.InProgress,
            IsPriorityAssessed = true,
            CreatedAt = new DateTime(2026, 10, 10, 6, 12, 0),
            DueDate = new DateTime(2026, 10, 12, 10, 0, 0),
            AuthorId = AuthorId,
            Author = new User { Id = AuthorId, Login = "olena" },
            DeveloperId = DeveloperId,
            Developer = new User { Id = DeveloperId, Login = "nazar" },
            Comments =
            [
                new Comment { Id = 1, Message = "<p>public note</p>", AuthorId = AuthorId, Author = new User { Login = "olena" }, CreatedAt = new DateTime(2026, 10, 10, 6, 14, 0) },
                new Comment { Id = 2, Message = "<p>secret note</p>", IsInternal = true, AuthorId = DeveloperId, Author = new User { Login = "nazar" }, CreatedAt = new DateTime(2026, 10, 10, 8, 40, 0) },
            ],
            Tags = [new Tag { Id = 1, Name = "reports", Color = "#f97316" }],
        };

        _tickets = new Mock<ITicketService>();
        _tickets.Setup(t => t.GetTicketByIdAsync(42)).ReturnsAsync(() => _ticket);
        _tickets.Setup(t => t.GetStatusHistoryAsync(42)).ReturnsAsync(
        [
            new TicketStatusHistory { Id = 1, TicketId = 42, OldStatus = TicketStatus.Open, NewStatus = TicketStatus.InProgress, ChangedAt = new DateTime(2026, 10, 10, 7, 5, 0), ChangedByUser = new User { Login = "nazar" } },
            new TicketStatusHistory { Id = 2, TicketId = 42, OldStatus = TicketStatus.New, NewStatus = TicketStatus.Open, ChangedAt = new DateTime(2026, 10, 10, 6, 30, 0) },
        ]);
        _tickets.Setup(t => t.DeleteTicketAsync(42)).ReturnsAsync(true);
        _tickets.Setup(t => t.UpdateTicketDatesAsync(42, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>())).ReturnsAsync(true);

        _tags = new Mock<ITagService>();
        _tags.Setup(t => t.GetAllTagsAsync()).ReturnsAsync(
        [
            new Tag { Id = 1, Name = "reports", Color = "#f97316" },
            new Tag { Id = 2, Name = "regression", Color = "#3b82f6" },
        ]);
        _tags.Setup(t => t.AssignTagToTicketAsync(42, It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(true);

        var workLogs = new Mock<IWorkLogService>();
        workLogs.Setup(w => w.GetWorkLogsByTicketIdAsync(It.IsAny<int>())).ReturnsAsync([]);
        var attachments = new Mock<IAttachmentService>();
        attachments.Setup(a => a.GetAttachmentsByTicketIdAsync(It.IsAny<int>())).ReturnsAsync([]);

        _host.Context.Services.AddSingleton(_tickets.Object);
        _host.Context.Services.AddSingleton(_tags.Object);
        _host.Context.Services.AddSingleton(new Mock<ITicketAssignmentService>().Object);
        _host.Context.Services.AddSingleton(new Mock<ICommentService>().Object);
        _host.Context.Services.AddSingleton(workLogs.Object);
        _host.Context.Services.AddSingleton(attachments.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private void SignIn(UserRole role, int id) =>
        _host.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = id, Login = "me", Role = role });

    private IRenderedComponent<TicketDetails> Render() =>
        _host.Context.RenderComponent<TicketDetails>(p => p.Add(x => x.Id, 42));

    [TestCase(UserRole.User, AuthorId, "/my-tickets")]
    [TestCase(UserRole.Developer, DeveloperId, "/tickets")]
    [TestCase(UserRole.Admin, 1, "/tickets")]
    public void Header_is_a_path_linking_to_the_roles_list_without_hero(UserRole role, int id, string listHref)
    {
        SignIn(role, id);

        var cut = Render();

        cut.Find("h1").TextContent.Should().Be("~/tickets/42");
        cut.Find("h1 a").GetAttribute("href").Should().Be(listHref);
        cut.Find("[data-ticket-title]").TextContent.Should().Be("Monthly report is empty");
        cut.Markup.Should().NotContain("page-hero").And.NotContain("gradient").And.NotContain("blur").And.NotContain("amber");
    }

    [Test]
    public void Status_steps_are_shown_to_the_assignee_with_one_primary()
    {
        SignIn(UserRole.Developer, DeveloperId);

        var cut = Render();

        var steps = cut.FindAll("[data-transition]");
        steps.Select(b => b.GetAttribute("data-transition")).Should().Equal("Testing", "CodeReview", "Resolved", "Closed");
        steps.First().ClassList.Should().Contain("btn-accent");
        cut.FindAll("[data-action=release]").Should().ContainSingle();
    }

    [TestCase(UserRole.Admin, 1)]
    [TestCase(UserRole.User, AuthorId)]
    [TestCase(UserRole.Developer, 99)]
    public void Status_steps_are_hidden_from_everyone_else(UserRole role, int id)
    {
        SignIn(role, id);

        var cut = Render();

        cut.FindAll("[data-transition]").Should().BeEmpty();
    }

    [Test]
    public void Status_step_saves_with_the_current_user_and_reloads_the_feed()
    {
        SignIn(UserRole.Developer, DeveloperId);
        _tickets.Setup(t => t.UpdateTicketStatusAsync(42, TicketStatus.Testing, DeveloperId)).ReturnsAsync(true);
        var cut = Render();

        cut.Find("[data-transition=Testing]").Click();

        _tickets.Verify(t => t.UpdateTicketStatusAsync(42, TicketStatus.Testing, DeveloperId), Times.Once);
        _tickets.Verify(t => t.GetStatusHistoryAsync(42), Times.AtLeast(2));
    }

    [Test]
    public void Free_ticket_offers_take_to_staff()
    {
        _ticket.DeveloperId = null;
        _ticket.Developer = null;
        SignIn(UserRole.Developer, DeveloperId);

        var cut = Render();

        cut.FindAll("[data-action=take]").Should().ContainSingle();
        cut.FindAll("[data-action=release]").Should().BeEmpty();
        cut.Find("[data-assignee]").TextContent.Should().Contain("table.unassigned");
    }

    [Test]
    public void Delete_asks_in_a_modal_and_goes_to_the_roles_list()
    {
        SignIn(UserRole.User, AuthorId);
        var cut = Render();

        cut.FindAll(".amodal-overlay").Should().BeEmpty();
        cut.Find("[data-action=delete]").Click();

        cut.Find(".amodal-overlay").TextContent.Should().Contain("details.deleteText");
        _tickets.Verify(t => t.DeleteTicketAsync(It.IsAny<int>()), Times.Never);

        cut.Find(".amodal-btn-save").Click();

        _tickets.Verify(t => t.DeleteTicketAsync(42), Times.Once);
        _host.Context.Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/my-tickets");
    }

    [Test]
    public void Delete_can_be_cancelled()
    {
        SignIn(UserRole.Admin, 1);
        var cut = Render();

        cut.Find("[data-action=delete]").Click();
        cut.Find(".amodal-btn-cancel").Click();

        cut.FindAll(".amodal-overlay").Should().BeEmpty();
        _tickets.Verify(t => t.DeleteTicketAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void Other_users_cannot_delete()
    {
        SignIn(UserRole.Developer, DeveloperId);

        var cut = Render();

        cut.FindAll("[data-action=delete]").Should().BeEmpty();
    }

    [Test]
    public void Users_see_a_read_only_panel_and_no_staff_sections()
    {
        SignIn(UserRole.User, AuthorId);

        var cut = Render();

        cut.FindAll("[data-properties-panel] [data-edit]").Should().BeEmpty();
        cut.FindAll("[data-add-tag], [data-remove-tag]").Should().BeEmpty();
        cut.Find("[data-due]").TextContent.Should().Be("12 Oct 2026, 13:00");
        cut.FindAll("[data-section=analytical-note], [data-section=worklog]").Should().BeEmpty();
    }

    [Test]
    public void Staff_edit_priority_dates_and_tags_in_the_panel()
    {
        SignIn(UserRole.Developer, DeveloperId);

        var cut = Render();

        cut.FindAll("[data-edit]").Select(e => e.GetAttribute("data-edit")).Should().Equal("priority", "start", "due");
        cut.FindAll("[data-section=analytical-note]").Should().ContainSingle();
        cut.FindAll("[data-section=worklog]").Should().ContainSingle();
    }

    [Test]
    public void Dates_show_local_time_and_are_saved_as_utc()
    {
        SignIn(UserRole.Developer, DeveloperId);
        var cut = Render();

        // Stored 10:00 UTC is 13:00 in Kyiv.
        cut.Find("[data-edit=due]").GetAttribute("value").Should().Be("2026-10-12T13:00");
        cut.FindAll("[data-save-dates]").Should().BeEmpty();

        cut.Find("[data-edit=due]").Change("2026-10-14T18:30");
        cut.Find("[data-save-dates]").Click();

        _tickets.Verify(t => t.UpdateTicketDatesAsync(42, null, new DateTime(2026, 10, 14, 15, 30, 0), DeveloperId), Times.Once);
    }

    [Test]
    public void Critical_priority_needs_confirmation_before_saving()
    {
        SignIn(UserRole.Developer, DeveloperId);
        var cut = Render();

        cut.FindAll("[data-priority-save]").Should().BeEmpty();
        cut.Find("[data-edit=priority]").Change("Critical");

        cut.Find("[data-priority-save] button").HasAttribute("disabled").Should().BeTrue();
        cut.Find("[data-confirm-critical]").Change(true);
        cut.Find("[data-priority-save] button").Click();

        _tickets.Verify(t => t.UpdateTicketPriorityAsync(42, TicketPriority.Critical), Times.Once);
    }

    [Test]
    public void Tag_dropdown_lists_only_unassigned_tags_and_closes_on_pick()
    {
        SignIn(UserRole.Admin, 1);
        var cut = Render();

        cut.FindAll("[data-tag-menu]").Should().BeEmpty();
        cut.Find("[data-add-tag]").Click();

        cut.FindAll("[data-tag-option]").Select(o => o.GetAttribute("data-tag-option")).Should().Equal("2");
        cut.Find("[data-tag-option='2']").Click();

        _tags.Verify(t => t.AssignTagToTicketAsync(42, 2, 1), Times.Once);
        cut.FindAll("[data-tag-menu]").Should().BeEmpty();
    }

    [Test]
    public void Tag_dropdown_closes_on_escape()
    {
        SignIn(UserRole.Admin, 1);
        var cut = Render();

        cut.Find("[data-add-tag]").Click();
        cut.Find("[data-add-tag]").KeyDown("Escape");

        cut.FindAll("[data-tag-menu]").Should().BeEmpty();
    }

    [Test]
    public void Feed_shows_system_rows_and_comments_with_internal_ones_for_staff()
    {
        SignIn(UserRole.Developer, DeveloperId);

        var cut = Render();

        var rows = cut.FindAll("[data-system-row]");
        rows.Select(r => r.Children[0].TextContent).Should().Equal("09:12", "09:30", "10:05");
        var text = rows.Select(r => r.Children[1].TextContent).ToList();
        text[0].Should().Be("@olena created · tickettype.bug · priority priority.medium");
        text[1].Should().Be("status status.new → status.open", because: "backfilled rows have no actor");
        text[2].Should().Be("@nazar status status.open → status.inprogress");
        cut.FindAll("[data-comment]").Select(c => c.GetAttribute("data-comment")).Should().Equal("1", "2");
        cut.Find("[data-comment='2']").TextContent.Should().Contain("[internal]");
        cut.FindAll("[data-composer] [role=radiogroup]").Should().ContainSingle();
    }

    [Test]
    public void Users_do_not_see_internal_comments_or_the_visibility_switch()
    {
        SignIn(UserRole.User, AuthorId);

        var cut = Render();

        cut.FindAll("[data-comment]").Select(c => c.GetAttribute("data-comment")).Should().Equal("1");
        cut.Markup.Should().NotContain("secret note");
        cut.FindAll("[data-composer] [role=radiogroup]").Should().BeEmpty();
    }

    [Test]
    public void Comment_actions_follow_ownership()
    {
        SignIn(UserRole.Admin, 1);

        var cut = Render();

        cut.FindAll("[data-edit-comment]").Should().BeEmpty("admins cannot edit other people's comments");
        cut.FindAll("[data-delete-comment]").Should().HaveCount(2);
    }

    [Test]
    public void Send_is_disabled_while_the_composer_is_empty()
    {
        SignIn(UserRole.User, AuthorId);

        var cut = Render();

        cut.Find("[data-send]").HasAttribute("disabled").Should().BeTrue();
    }

    [Test]
    public void Triage_notice_is_a_plain_accent_row_for_staff()
    {
        _ticket.IsPriorityAssessed = false;
        SignIn(UserRole.Developer, DeveloperId);

        var cut = Render();

        var notice = cut.Find("[data-triage-notice]");
        notice.ClassList.Should().Contain("border-accent");
        notice.OuterHtml.Should().NotContain("pulse").And.NotContain("amber");
        cut.FindAll("[data-priority-save]").Should().ContainSingle("an unassessed priority can be saved as is");
    }

    [Test]
    public void Missing_ticket_shows_an_empty_state_with_a_link_to_the_list()
    {
        _ticket = null!;
        SignIn(UserRole.User, AuthorId);

        var cut = Render();

        cut.Markup.Should().Contain("details.notFound");
        cut.Find("p.font-mono button").Click();
        _host.Context.Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/my-tickets");
    }
}
