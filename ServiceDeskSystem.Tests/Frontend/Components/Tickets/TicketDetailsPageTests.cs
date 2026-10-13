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
using ServiceDeskSystem.Components.UI.Elements;
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
    private Mock<ICommentService> _comments;
    private Mock<ITicketAssignmentService> _assignment;
    private Mock<IWorkLogService> _workLogs;
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
        _tickets.Setup(t => t.UpdateTicketDatesAsync(42, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(TicketUpdateResult.Updated);

        _tags = new Mock<ITagService>();
        _tags.Setup(t => t.GetAllTagsAsync()).ReturnsAsync(
        [
            new Tag { Id = 1, Name = "reports", Color = "#f97316" },
            new Tag { Id = 2, Name = "regression", Color = "#3b82f6" },
        ]);
        _tags.Setup(t => t.AssignTagToTicketAsync(42, It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(true);

        _workLogs = new Mock<IWorkLogService>();
        _workLogs.Setup(w => w.GetWorkLogsByTicketIdAsync(It.IsAny<int>())).ReturnsAsync([]);
        _comments = new Mock<ICommentService>();
        _comments.Setup(c => c.AddCommentAsync(It.IsAny<Comment>())).ReturnsAsync((Comment c) => c);
        _assignment = new Mock<ITicketAssignmentService>();
        var attachments = new Mock<IAttachmentService>();
        attachments.Setup(a => a.GetAttachmentsByTicketIdAsync(It.IsAny<int>())).ReturnsAsync([]);

        _host.Context.Services.AddSingleton(_tickets.Object);
        _host.Context.Services.AddSingleton(_tags.Object);
        _host.Context.Services.AddSingleton(_assignment.Object);
        _host.Context.Services.AddSingleton(_comments.Object);
        _host.Context.Services.AddSingleton(_workLogs.Object);
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
        _tickets.Setup(t => t.UpdateTicketStatusAsync(42, TicketStatus.Testing, DeveloperId, It.IsAny<int?>())).ReturnsAsync(TicketUpdateResult.Updated);
        var cut = Render();

        cut.Find("[data-transition=Testing]").Click();

        _tickets.Verify(t => t.UpdateTicketStatusAsync(42, TicketStatus.Testing, DeveloperId, It.IsAny<int?>()), Times.Once);
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

        cut.FindAll("[data-modal]").Should().BeEmpty();
        cut.Find("[data-action=delete]").Click();

        cut.Find("[data-modal]").TextContent.Should().Contain("details.deleteText");
        _tickets.Verify(t => t.DeleteTicketAsync(It.IsAny<int>()), Times.Never);

        cut.Find("[data-modal-save]").Click();

        _tickets.Verify(t => t.DeleteTicketAsync(42), Times.Once);
        _host.Context.Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/my-tickets");
    }

    [Test]
    public void Delete_can_be_cancelled()
    {
        SignIn(UserRole.Admin, 1);
        var cut = Render();

        cut.Find("[data-action=delete]").Click();
        cut.Find("[data-modal-cancel]").Click();

        cut.FindAll("[data-modal]").Should().BeEmpty();
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

        _tickets.Verify(t => t.UpdateTicketDatesAsync(42, null, new DateTime(2026, 10, 14, 15, 30, 0), DeveloperId, It.IsAny<int?>()), Times.Once);
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

        _tickets.Verify(t => t.UpdateTicketPriorityAsync(42, TicketPriority.Critical, It.IsAny<int?>()), Times.Once);
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
        text[0].Should().Be("@olena created · tickettype.bug", because: "the original priority is not stored");
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

    [Test]
    public void Leaving_while_the_ticket_loads_does_not_throw()
    {
        // Logging out on this page re-creates it and navigates away before the load finishes.
        SignIn(UserRole.Developer, DeveloperId);
        var load = new TaskCompletionSource<Ticket?>();
        _tickets.Setup(t => t.GetTicketByIdAsync(42)).Returns(load.Task);
        Render();

        _host.Context.DisposeComponents();
        _host.Context.Renderer.Dispatcher.InvokeAsync(() => load.SetResult(_ticket)).GetAwaiter().GetResult();

        _host.Context.Renderer.UnhandledException.IsCompleted.Should().BeFalse();
    }

    private static void Type(IRenderedComponent<TicketDetails> cut, IRenderedComponent<RichTextEditor> editor, string html) =>
        cut.InvokeAsync(() => editor.Instance.UpdateContent(html)).GetAwaiter().GetResult();

    private static IRenderedComponent<RichTextEditor> Composer(IRenderedComponent<TicketDetails> cut) =>
        cut.FindComponents<RichTextEditor>().Single(e => e.Instance.Borderless);

    private static Ticket CopyOf(Ticket t) => new ()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Type = t.Type,
        Priority = t.Priority,
        Status = t.Status,
        IsPriorityAssessed = t.IsPriorityAssessed,
        CreatedAt = t.CreatedAt,
        StartDate = t.StartDate,
        DueDate = t.DueDate,
        AuthorId = t.AuthorId,
        Author = t.Author,
        DeveloperId = t.DeveloperId,
        Developer = t.Developer,
        Comments = t.Comments.ToList(),
        Tags = t.Tags.ToList(),
    };

    [Test]
    public void Refresh_keeps_every_unsaved_edit()
    {
        SignIn(UserRole.Developer, DeveloperId);
        var cut = Render();

        cut.Find("[data-edit=priority]").Change("High");
        cut.Find("[data-edit=due]").Change("2026-10-20T09:15");
        Type(cut, Composer(cut), "<p>draft reply</p>");
        cut.Find("[data-comment='2'] [data-edit-comment]").Click();
        Type(cut, cut.FindComponents<RichTextEditor>().Single(e => !e.Instance.Borderless), "<p>edited secret</p>");
        cut.Find("[data-edit-note]").Click();
        cut.Find("[data-section=analytical-note] textarea").Change("half-written note");

        _ticket = CopyOf(_ticket);
        cut.InvokeAsync(() => cut.Instance.RefreshAsync()).GetAwaiter().GetResult();

        _tickets.Verify(t => t.GetTicketByIdAsync(42), Times.AtLeast(2));
        cut.Find("[data-edit=priority]").GetAttribute("value").Should().Be("High");
        cut.Find("[data-edit=due]").GetAttribute("value").Should().Be("2026-10-20T09:15");
        cut.FindAll("[data-save-dates]").Should().ContainSingle();
        Composer(cut).Instance.Value.Should().Be("<p>draft reply</p>");
        cut.FindComponents<RichTextEditor>().Single(e => !e.Instance.Borderless).Instance.Value.Should().Be("<p>edited secret</p>");
        cut.Find("[data-section=analytical-note] textarea").GetAttribute("value").Should().Be("half-written note");
    }

    [Test]
    public void Saving_one_date_sends_the_freshly_stored_value_of_the_other()
    {
        SignIn(UserRole.Developer, DeveloperId);
        _ticket.StartDate = new DateTime(2026, 10, 10, 5, 0, 37);
        var cut = Render();

        // Someone else moved the start (with seconds) after the page loaded.
        var fresh = CopyOf(_ticket);
        fresh.StartDate = new DateTime(2026, 10, 11, 4, 30, 59);
        _ticket = fresh;

        cut.Find("[data-edit=due]").Change("2026-10-14T18:30");
        cut.Find("[data-save-dates]").Click();

        _tickets.Verify(t => t.UpdateTicketDatesAsync(42, new DateTime(2026, 10, 11, 4, 30, 59), new DateTime(2026, 10, 14, 15, 30, 0), DeveloperId, It.IsAny<int?>()), Times.Once);
    }

    [Test]
    public void Failed_saves_keep_the_edits_and_say_so()
    {
        SignIn(UserRole.Developer, DeveloperId);
        _tickets.Setup(t => t.UpdateTicketPriorityAsync(42, It.IsAny<TicketPriority>(), It.IsAny<int?>())).ReturnsAsync(TicketUpdateResult.NotFound);
        _tickets.Setup(t => t.UpdateTicketDatesAsync(42, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(TicketUpdateResult.NotFound);
        var cut = Render();

        cut.Find("[data-edit=priority]").Change("High");
        cut.Find("[data-priority-save] button").Click();
        cut.Find("[data-edit=due]").Change("2026-10-14T18:30");
        cut.Find("[data-save-dates]").Click();

        cut.Find("[data-edit=priority]").GetAttribute("value").Should().Be("High");
        cut.FindAll("[data-priority-save]").Should().ContainSingle();
        cut.Find("[data-edit=due]").GetAttribute("value").Should().Be("2026-10-14T18:30");
        cut.FindAll("[data-save-dates]").Should().ContainSingle();
        _host.ToastService.Verify(t => t.ShowToastAsync("details.saveFailed", ServiceDeskSystem.Application.Services.Toasts.Models.ToastType.Error), Times.Exactly(2));
    }

    [Test]
    public void A_failed_comment_edit_keeps_the_editor_open()
    {
        SignIn(UserRole.Developer, DeveloperId);
        _comments.Setup(c => c.UpdateCommentAsync(2, It.IsAny<string>(), DeveloperId, false)).ReturnsAsync((Comment?)null);
        var cut = Render();

        cut.Find("[data-comment='2'] [data-edit-comment]").Click();
        Type(cut, cut.FindComponents<RichTextEditor>().Single(e => !e.Instance.Borderless), "<p>new text</p>");
        cut.Find("[data-save-comment]").Click();

        _comments.Verify(c => c.UpdateCommentAsync(2, "<p>new text</p>", DeveloperId, false), Times.Once);
        cut.FindAll("[data-save-comment]").Should().ContainSingle();
    }

    [Test]
    public void Users_cannot_post_internal_comments()
    {
        SignIn(UserRole.User, AuthorId);
        var cut = Render();

        Type(cut, Composer(cut), "<p>any news?</p>");
        cut.Find("[data-send]").Click();

        _comments.Verify(c => c.AddCommentAsync(It.Is<Comment>(m => m.Message == "<p>any news?</p>" && !m.IsInternal && m.AuthorId == AuthorId && m.TicketId == 42)), Times.Once);
        Composer(cut).Instance.Value.Should().BeEmpty();
    }

    [Test]
    public void Staff_can_post_internal_comments()
    {
        SignIn(UserRole.Developer, DeveloperId);
        var cut = Render();

        Type(cut, Composer(cut), "<p>root cause found</p>");
        cut.FindAll("[data-composer] button[role=radio]").Last().Click();
        cut.Find("[data-send]").Click();

        _comments.Verify(c => c.AddCommentAsync(It.Is<Comment>(m => m.IsInternal)), Times.Once);
    }

    [Test]
    public void Take_and_release_call_the_assignment_service()
    {
        _ticket.DeveloperId = null;
        _ticket.Developer = null;
        _assignment.Setup(a => a.AssignDeveloperAsync(42, DeveloperId, It.IsAny<int?>())).ReturnsAsync(TicketUpdateResult.Updated);
        SignIn(UserRole.Developer, DeveloperId);
        var cut = Render();

        cut.Find("[data-action=take]").Click();

        _assignment.Verify(a => a.AssignDeveloperAsync(42, DeveloperId, It.IsAny<int?>()), Times.Once);

        _ticket = CopyOf(_ticket);
        _ticket.DeveloperId = DeveloperId;
        _ticket.Developer = new User { Id = DeveloperId, Login = "nazar" };
        _assignment.Setup(a => a.UnassignDeveloperAsync(42, It.IsAny<int?>())).ReturnsAsync(TicketUpdateResult.Updated);
        cut.InvokeAsync(() => cut.Instance.RefreshAsync()).GetAwaiter().GetResult();

        cut.Find("[data-action=release]").Click();

        _assignment.Verify(a => a.UnassignDeveloperAsync(42, It.IsAny<int?>()), Times.Once);
    }

    [Test]
    public void Taking_a_ticket_someone_else_just_changed_passes_the_loaded_version_then_reloads_and_warns()
    {
        _ticket.DeveloperId = null;
        _ticket.Developer = null;
        _ticket.Version = 7;
        SignIn(UserRole.Developer, DeveloperId);
        var cut = Render();

        // Meanwhile another developer takes it.
        var taken = CopyOf(_ticket);
        taken.DeveloperId = 99;
        taken.Developer = new User { Id = 99, Login = "olha" };
        taken.Version = 8;
        _ticket = taken;
        _assignment.Setup(a => a.AssignDeveloperAsync(42, DeveloperId, 7)).ReturnsAsync(TicketUpdateResult.Conflict);

        cut.Find("[data-action=take]").Click();

        _assignment.Verify(a => a.AssignDeveloperAsync(42, DeveloperId, 7), Times.Once);
        _host.ToastService.Verify(t => t.ShowToastAsync("details.conflict", ServiceDeskSystem.Application.Services.Toasts.Models.ToastType.Warning), Times.Once);
        cut.FindAll("[data-action=take]").Should().BeEmpty();
        cut.Markup.Should().Contain("olha");
    }

    [Test]
    public void Work_log_day_is_saved_as_utc_of_local_midnight()
    {
        SignIn(UserRole.Developer, DeveloperId);
        _workLogs.Setup(w => w.AddWorkLogAsync(42, DeveloperId, 30, It.IsAny<DateTime>(), "fixed mapper", It.IsAny<DateOnly?>()))
            .ReturnsAsync((true, (string?)null));
        var cut = Render();

        cut.Find("[data-worklog-form] input[type=date]").Change("2026-10-12");
        cut.Find("[data-worklog-form] input[type=number]").Change("30");
        cut.Find("[data-worklog-form] input[type=text]").Change("fixed mapper");
        cut.Find("[data-add-worklog]").Click();

        _workLogs.Verify(w => w.AddWorkLogAsync(42, DeveloperId, 30, new DateTime(2026, 10, 11, 21, 0, 0, DateTimeKind.Utc), "fixed mapper", new DateOnly(2026, 10, 12)), Times.Once);
    }

    [Test]
    public void Day_separator_follows_the_local_date()
    {
        // 21:30 UTC on 10 Oct is 00:30 on 11 Oct in Kyiv.
        _ticket.Comments.Add(new Comment { Id = 3, Message = "<p>late</p>", AuthorId = AuthorId, Author = new User { Login = "olena" }, CreatedAt = new DateTime(2026, 10, 10, 21, 30, 0) });
        SignIn(UserRole.User, AuthorId);

        var cut = Render();

        cut.FindAll("[data-day]").Select(d => d.TextContent).Should().Equal("10 Oct 2026", "11 Oct 2026");
        cut.Find("[data-comment='3'] span").TextContent.Should().Be("00:30");
    }
}
