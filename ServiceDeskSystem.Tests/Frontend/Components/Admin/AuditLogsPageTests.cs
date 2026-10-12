using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Components.Pages.Admin;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Admin;

[TestFixture]
public class AuditLogsPageTests
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    private ComponentTestHost _host;
    private Mock<IAuditService> _audit;
    private List<AuditLog> _logs;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();

        // UTC+14: the user's day differs from the server's and from UTC most of the time.
        _host.TimeZone.Set("Pacific/Kiritimati");

        _host.Localization.Setup(l => l.Translate("audit.shownOf")).Returns("{0} of {1}");
        _host.Localization.Setup(l => l.Translate("audit.showMore")).Returns("show {0} more");
        _host.Localization.Setup(l => l.Translate("audit.realtime.newEvents")).Returns("{0} new events — show");

        _logs = [];
        _audit = new Mock<IAuditService>();
        _audit.Setup(a => a.GetLatestLogsAsync(It.IsAny<int>())).ReturnsAsync(() => _logs.ToList());
        _host.Context.Services.AddSingleton(_audit.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private DateOnly Today => _host.TimeZone.Today;

    private DateTime Utc(DateOnly day, int hour, int minute = 0) =>
        _host.TimeZone.ToUtc(day.ToDateTime(new TimeOnly(hour, minute)));

    private static AuditLog Log(int id, DateTime utc, string action = "UPDATE_TICKET", string entity = "Ticket", string entityId = "42", User? user = null, string? changes = null) =>
        new () { Id = id, Timestamp = utc, Action = action, EntityName = entity, EntityId = entityId, User = user, UserId = user?.Id, Changes = changes };

    private static readonly User AdminUser = new () { Id = 1, Login = "admin", Role = UserRole.Admin };

    private IRenderedComponent<TestAuditLogs> Render() => _host.Context.RenderComponent<TestAuditLogs>();

    private static IElement Row(IRenderedComponent<TestAuditLogs> cut, int id) => cut.Find($"[data-event='{id}']");

    private static IElement Option(IRenderedComponent<TestAuditLogs> cut, string group, string text) =>
        cut.FindAll($"[role=radiogroup][aria-label='{group}'] button").Single(b => b.TextContent.Trim() == text);

    [Test]
    public void Non_admins_see_access_denied_and_nothing_loads()
    {
        _host.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = 5, Login = "dev", Role = UserRole.Developer });

        var cut = Render();

        cut.Find("[data-access-denied]").TextContent.Should().Contain("admin.accessDenied");
        cut.FindAll("[data-log]").Should().BeEmpty();
        cut.FindAll("[data-action=clear]").Should().BeEmpty();
        _audit.Verify(a => a.GetLatestLogsAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void Header_shows_the_path_and_the_live_status()
    {
        var cut = Render();

        cut.Find("h1").TextContent.Replace(" ", string.Empty).Should().Be("~/audit-logs");
        cut.Find("[data-live]").GetAttribute("data-live").Should().Be("live");
        cut.Find("[data-auto-refresh]").TextContent.Should().Contain("auto 20s");

        cut.Find("[data-action=pause]").Click();

        cut.Find("[data-auto-refresh]").TextContent.Should().Contain("audit.autorefreshpaused");
    }

    [Test]
    public void Rows_are_grouped_by_local_day_with_today_and_yesterday_headings()
    {
        var earlier = Today.AddDays(-3);
        _logs =
        [
            Log(3, Utc(Today, 9)),
            Log(2, Utc(Today.AddDays(-1), 23, 30)),
            Log(1, Utc(earlier, 12)),
        ];

        var cut = Render();

        var headings = cut.FindAll("[data-day] h2").Select(h => h.TextContent.Trim()).ToList();
        headings.Should().Equal(
            $"# audit.today · {Today.ToString("d MMM", English)}",
            $"# audit.yesterday · {Today.AddDays(-1).ToString("d MMM", English)}",
            $"# {earlier.ToString("d MMM yyyy", English)}");
        cut.Find("[data-day] [data-event='2'] [data-col=time]").TextContent.Should().Be("23:30:00");
    }

    [Test]
    public void Rows_show_level_user_with_system_fallback_and_entity()
    {
        _logs =
        [
            Log(3, Utc(Today, 9), action: "DELETE_USER", entity: "User", entityId: "9"),
            Log(2, Utc(Today, 8), action: "LOGIN_FAILED", entity: "User", entityId: "1", user: AdminUser),
            Log(1, Utc(Today, 7), action: "UPDATE_SETTINGS", entity: "Settings", entityId: string.Empty, user: AdminUser),
        ];

        var cut = Render();

        var critical = Row(cut, 3);
        critical.QuerySelector("[data-col=level]")!.TextContent.Should().Be("CRIT");
        critical.QuerySelector("[data-col=level]")!.ClassList.Should().Contain("text-danger");
        critical.QuerySelector("[data-col=user]")!.TextContent.Should().Be("@system");
        critical.QuerySelector("[data-col=entity]")!.TextContent.Should().Be("User#9");

        Row(cut, 2).QuerySelector("[data-col=level]")!.TextContent.Should().Be("WARN");
        Row(cut, 2).QuerySelector("[data-col=user]")!.TextContent.Should().Be("@admin");
        Row(cut, 1).QuerySelector("[data-col=level]")!.TextContent.Should().Be("info");
        Row(cut, 1).QuerySelector("[data-col=entity]")!.TextContent.Should().Be("Settings");
    }

    [Test]
    public void Figures_count_today_in_the_users_zone()
    {
        _logs =
        [
            Log(3, Utc(Today, 0, 30), action: "DELETE_USER", user: AdminUser),
            Log(2, Utc(Today.AddDays(-1), 23, 30), action: "LOGIN_FAILED"),
            Log(1, Utc(Today.AddDays(-1), 10)),
        ];

        var cut = Render();

        cut.Find("[data-figure=total] p").TextContent.Should().Be("3");
        cut.Find("[data-figure=today] p").TextContent.Should().Be("1");
        cut.Find("[data-figure=severity] p").TextContent.Should().MatchRegex(@"^\s*1\s*/\s*1\s*$");
        cut.Find("[data-figure=users] p").TextContent.Should().Be("1");
    }

    [Test]
    public void Clicking_a_row_shows_the_detail_with_the_diff_and_a_second_click_hides_it()
    {
        var diff = new AuditChangePayload
        {
            Summary = "Updated SLA policy High",
            IpAddress = "10.0.0.7",
            Diff = [new ("ResolutionMinutes", "2880", "1440"), new ("Description", null, "fast lane")],
        };
        _logs = [Log(1, Utc(Today, 9), action: "UPDATE_SLA_POLICY", entity: "SlaPolicy", entityId: "2", user: AdminUser, changes: diff.ToJson())];
        var cut = Render();

        Row(cut, 1).Click();

        Row(cut, 1).GetAttribute("aria-expanded").Should().Be("true");
        var detail = cut.Find("[data-detail='1']");
        detail.TextContent.Should().Contain("@admin").And.Contain("id 1 · 10.0.0.7").And.Contain("UTC");
        detail.QuerySelectorAll("[data-diff-old]").Select(e => e.TextContent).Should().Equal("- 2880", "- «null»");
        detail.QuerySelectorAll("[data-diff-new]").Select(e => e.TextContent).Should().Equal("+ 1440", "+ fast lane");
        detail.QuerySelectorAll("[data-raw]").Should().BeEmpty();

        Row(cut, 1).Click();

        cut.FindAll("[data-detail]").Should().BeEmpty();
        Row(cut, 1).GetAttribute("aria-expanded").Should().Be("false");
    }

    [Test]
    public void Only_one_event_is_open_at_a_time()
    {
        _logs = [Log(2, Utc(Today, 9)), Log(1, Utc(Today, 8), changes: "Status changed from New to Open")];
        var cut = Render();

        Row(cut, 2).Click();
        Row(cut, 1).Click();

        cut.FindAll("[data-detail]").Should().ContainSingle()
            .Which.GetAttribute("data-detail").Should().Be("1");
        cut.Find("[data-raw]").TextContent.Should().Be("Status changed from New to Open");
        Row(cut, 2).GetAttribute("aria-expanded").Should().Be("false");
    }

    [Test]
    public void The_open_event_uses_the_accent_tint_in_light_theme()
    {
        _logs = [Log(2, Utc(Today, 9)), Log(1, Utc(Today, 8))];
        var cut = Render();

        Row(cut, 1).Click();

        Row(cut, 1).ClassList.Should().Contain(["light:bg-accent/10", "bg-surface"]).And.NotContain("light:hover:bg-accent/7");
        cut.Find("[data-detail='1']").ClassList.Should().Contain("light:bg-accent/10");
        Row(cut, 2).ClassList.Should().NotContain("light:bg-accent/10").And.Contain("light:hover:bg-accent/7");
    }

    [Test]
    public void Ticket_events_link_to_the_ticket()
    {
        _logs = [Log(1, Utc(Today, 9), entity: "Ticket", entityId: "42")];
        var cut = Render();

        Row(cut, 1).Click();

        cut.Find("[data-entity-link]").GetAttribute("href").Should().Be("/ticket/42");
    }

    [Test]
    public void Show_more_adds_50_and_a_filter_change_resets_to_50()
    {
        _logs = Enumerable.Range(1, 120).Select(i => Log(i, Utc(Today, 0).AddSeconds(-i), action: "LOGIN")).ToList();
        var cut = Render();

        cut.FindAll("[data-event]").Should().HaveCount(50);
        cut.Find("[data-action=more]").TextContent.Should().Contain("show 50 more").And.Contain("50 of 120");

        cut.Find("[data-action=more]").Click();
        cut.FindAll("[data-event]").Should().HaveCount(100);
        cut.Find("[data-action=more]").TextContent.Should().Contain("show 20 more").And.Contain("100 of 120");

        cut.Find("[data-action=more]").Click();
        cut.FindAll("[data-event]").Should().HaveCount(120);
        cut.FindAll("[data-action=more]").Should().BeEmpty();

        cut.Find("[data-search]").Input("login");

        cut.FindAll("[data-event]").Should().HaveCount(50);
    }

    [Test]
    public void Severity_and_period_filters_narrow_the_log_and_reset_clears_them()
    {
        _logs =
        [
            Log(3, Utc(Today, 9), action: "DELETE_TAG"),
            Log(2, Utc(Today.AddDays(-10), 9), action: "LOGIN_FAILED"),
            Log(1, Utc(Today.AddDays(-1), 9), action: "UPDATE_TICKET"),
        ];
        var cut = Render();
        cut.FindAll("[data-shown]").Should().BeEmpty();

        Option(cut, "audit.severity", "crit").Click();

        cut.FindAll("[data-event]").Select(e => e.GetAttribute("data-event")).Should().Equal("3");
        cut.Find("[data-shown]").TextContent.Should().Contain("1 of 3");

        Option(cut, "audit.severity", "all").Click();
        Option(cut, "audit.period", "today").Click();
        cut.FindAll("[data-event]").Select(e => e.GetAttribute("data-event")).Should().Equal("3");

        Option(cut, "audit.period", "7d").Click();
        cut.FindAll("[data-event]").Select(e => e.GetAttribute("data-event")).Should().Equal("3", "1");

        Option(cut, "audit.period", "30d").Click();
        cut.FindAll("[data-event]").Should().HaveCount(3);

        Option(cut, "audit.severity", "warn").Click();
        cut.FindAll("[data-event]").Select(e => e.GetAttribute("data-event")).Should().Equal("2");

        cut.Find("[data-action=reset]").Click();

        cut.FindAll("[data-event]").Should().HaveCount(3);
        cut.FindAll("[data-shown]").Should().BeEmpty();
        Option(cut, "audit.severity", "all").GetAttribute("aria-checked").Should().Be("true");
        Option(cut, "audit.period", "all").GetAttribute("aria-checked").Should().Be("true");
    }

    [Test]
    public void An_empty_filtered_log_offers_a_reset()
    {
        _logs = [Log(1, Utc(Today, 9))];
        var cut = Render();

        cut.Find("[data-search]").Input("nothing matches this");

        cut.Find("[data-log]").TextContent.Should().Contain("audit.noLogs");
        cut.Find("[data-log] button").Click();
        cut.FindAll("[data-event]").Should().ContainSingle();
    }

    [Test]
    public void Clear_shows_the_prompt_without_calling_the_service()
    {
        _logs = [Log(1, Utc(Today, 9))];
        var cut = Render();

        cut.Find("[data-action=clear]").Click();

        var prompt = cut.Find("[data-clear-prompt]");
        prompt.TextContent.Should().Contain("rm").And.Contain("audit.log?").And.Contain("admin.deletenote");
        cut.Find("[data-action=clear]").ParentElement!.QuerySelector("[data-clear-prompt]")
            .Should().BeNull("the prompt sits under the header, not among the actions");
        cut.Find("[data-action=clear]").HasAttribute("disabled").Should().BeTrue();
        cut.Find("[data-action=clear]").GetAttribute("aria-expanded").Should().Be("true");
        _audit.Verify(a => a.ClearAllLogsAsync(), Times.Never);
        _host.Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-clear-prompt] [data-action=keep]"));
    }

    [Test]
    public void N_closes_the_clear_prompt()
    {
        var cut = Render();
        cut.Find("[data-action=clear]").Click();

        cut.Find("[data-clear-prompt] [data-action=keep]").Click();

        cut.FindAll("[data-clear-prompt]").Should().BeEmpty();
        cut.Find("[data-action=clear]").HasAttribute("disabled").Should().BeFalse();
        _audit.Verify(a => a.ClearAllLogsAsync(), Times.Never);
    }

    [Test]
    public void Y_clears_the_log_once()
    {
        _logs = [Log(1, Utc(Today, 9))];
        _audit.Setup(a => a.ClearAllLogsAsync()).Callback(() => _logs = []).Returns(Task.CompletedTask);
        var cut = Render();
        cut.Find("[data-action=clear]").Click();

        cut.Find("[data-clear-prompt] [data-action=keep]").KeyDown(key: "y");

        _audit.Verify(a => a.ClearAllLogsAsync(), Times.Once);
        cut.FindAll("[data-clear-prompt]").Should().BeEmpty();
        cut.FindAll("[data-event]").Should().BeEmpty();
    }

    [Test]
    public void New_events_reload_the_list_when_nothing_holds_it()
    {
        _logs = [Log(1, Utc(Today, 9))];
        var cut = Render();
        _logs = [Log(2, Utc(Today, 10)), .. _logs];

        cut.InvokeAsync(() => cut.Instance.RaiseAuditLogsChangedAsync()).GetAwaiter().GetResult();

        cut.FindAll("[data-pending-events]").Should().BeEmpty();
        cut.FindAll("[data-event]").Should().HaveCount(2);
    }

    [Test]
    public void Pending_events_bar_applies_the_held_events()
    {
        _logs = [Log(1, Utc(Today, 9))];
        var cut = Render();
        Row(cut, 1).Click();
        _logs = [Log(3, Utc(Today, 11)), Log(2, Utc(Today, 10)), .. _logs];

        cut.InvokeAsync(() => cut.Instance.RaiseAuditLogsChangedAsync()).GetAwaiter().GetResult();
        cut.InvokeAsync(() => cut.Instance.RaiseAuditLogsChangedAsync()).GetAwaiter().GetResult();

        cut.FindAll("[data-event]").Should().ContainSingle();
        var bar = cut.Find("[data-pending-events]");
        bar.TextContent.Should().Contain("2 new events — show");

        bar.Click();

        cut.FindAll("[data-pending-events]").Should().BeEmpty();
        cut.FindAll("[data-event]").Should().HaveCount(3);
        _audit.Verify(a => a.GetLatestLogsAsync(It.IsAny<int>()), Times.Exactly(2));
    }

    [TestCase(-6, true)]
    [TestCase(-7, false)]
    public void Seven_days_are_today_and_the_six_days_before(int days, bool listed)
    {
        _logs = [Log(1, Utc(Today.AddDays(days), 0, 1))];
        var cut = Render();

        Option(cut, "audit.period", "7d").Click();

        cut.FindAll("[data-event]").Should().HaveCount(listed ? 1 : 0);
    }

    [TestCase(-29, true)]
    [TestCase(-30, false)]
    public void Thirty_days_are_today_and_the_29_days_before(int days, bool listed)
    {
        _logs = [Log(1, Utc(Today.AddDays(days), 0, 1))];
        var cut = Render();

        Option(cut, "audit.period", "30d").Click();

        cut.FindAll("[data-event]").Should().HaveCount(listed ? 1 : 0);
    }

    [Test]
    public void Escape_closes_the_clear_prompt()
    {
        var cut = Render();
        cut.Find("[data-action=clear]").Click();

        cut.Find("[data-clear-prompt] [data-action=keep]").KeyDown(key: "Escape");

        cut.FindAll("[data-clear-prompt]").Should().BeEmpty();
        _audit.Verify(a => a.ClearAllLogsAsync(), Times.Never);
    }

    [Test]
    public void Cancel_is_ignored_while_the_log_is_clearing()
    {
        var clearing = new TaskCompletionSource();
        _audit.Setup(a => a.ClearAllLogsAsync()).Returns(clearing.Task);
        var cut = Render();
        cut.Find("[data-action=clear]").Click();
        cut.Find("[data-clear-prompt] [data-action=confirm]").Click();

        cut.Find("[data-clear-prompt] [data-action=keep]").Click();
        cut.Find("[data-clear-prompt] [data-action=keep]").KeyDown(key: "Escape");

        cut.FindAll("[data-clear-prompt]").Should().ContainSingle();
        cut.Find("[data-action=clear]").HasAttribute("disabled").Should().BeTrue();

        cut.InvokeAsync(clearing.SetResult).GetAwaiter().GetResult();

        cut.WaitForAssertion(() => cut.FindAll("[data-clear-prompt]").Should().BeEmpty());
        _audit.Verify(a => a.ClearAllLogsAsync(), Times.Once);
    }

    [Test]
    public void A_failed_clear_shows_an_error_toast_and_keeps_the_log()
    {
        _logs = [Log(1, Utc(Today, 9))];
        _audit.Setup(a => a.ClearAllLogsAsync()).ThrowsAsync(new InvalidOperationException("db down"));
        var cut = Render();
        cut.Find("[data-action=clear]").Click();

        cut.Find("[data-clear-prompt] [data-action=confirm]").Click();

        _host.ToastService.Verify(
            t => t.ShowToastAsync(
                It.Is<string>(m => m.Contains("audit.clearFailed") && m.Contains("db down")),
                ServiceDeskSystem.Application.Services.Toasts.Models.ToastType.Error,
                It.IsAny<int>()),
            Times.Once);
        cut.FindAll("[data-clear-prompt]").Should().BeEmpty();
        cut.FindAll("[data-event]").Should().ContainSingle();
    }

    [Test]
    public void An_open_event_that_is_gone_after_a_reload_no_longer_holds_live_events()
    {
        _logs = [Log(5, Utc(Today, 9))];
        var cut = Render();
        Row(cut, 5).Click();

        // Another admin clears the log: the event is held, then applied, and the open id is gone.
        _logs = [Log(6, Utc(Today, 10))];
        Raise(cut);
        cut.Find("[data-pending-events]").Click();

        cut.FindAll("[data-detail]").Should().BeEmpty();
        _logs = [Log(7, Utc(Today, 11)), .. _logs];
        Raise(cut);

        cut.FindAll("[data-pending-events]").Should().BeEmpty();
        cut.FindAll("[data-event]").Select(e => e.GetAttribute("data-event")).Should().Equal("7", "6");
    }

    [Test]
    public void A_filter_change_closes_the_open_event()
    {
        _logs = [Log(2, Utc(Today, 9), action: "LOGIN"), Log(1, Utc(Today, 8))];
        var cut = Render();
        Row(cut, 1).Click();

        cut.Find("[data-search]").Input("login");
        cut.Find("[data-action=reset]").Click();

        cut.FindAll("[data-detail]").Should().BeEmpty();
        Row(cut, 1).GetAttribute("aria-expanded").Should().Be("false");
    }

    [Test]
    public void Auto_refresh_follows_the_same_hold_rules_as_live_events()
    {
        _logs = [Log(1, Utc(Today, 9))];
        var cut = Render();
        Option(cut, "audit.severity", "info").Click();
        _logs = [Log(2, Utc(Today, 10)), .. _logs];

        cut.InvokeAsync(() => cut.Instance.TickAsync()).GetAwaiter().GetResult();

        cut.FindAll("[data-event]").Should().ContainSingle();
        _audit.Verify(a => a.GetLatestLogsAsync(It.IsAny<int>()), Times.Once);

        Option(cut, "audit.severity", "all").Click();
        cut.InvokeAsync(() => cut.Instance.TickAsync()).GetAwaiter().GetResult();

        cut.FindAll("[data-event]").Should().HaveCount(2);
    }

    [Test]
    public void Any_reload_clears_the_pending_events_bar()
    {
        _logs = [Log(1, Utc(Today, 9))];
        var cut = Render();
        cut.Find("[data-action=pause]").Click();
        _logs = [Log(2, Utc(Today, 10)), .. _logs];
        Raise(cut);
        cut.Find("[data-pending-events]").TextContent.Should().Contain("1 new events");

        cut.Find("[data-action=refresh]").Click();

        cut.FindAll("[data-pending-events]").Should().BeEmpty();
        cut.FindAll("[data-event]").Should().HaveCount(2);
    }

    [Test]
    public void A_failed_hub_start_shows_offline()
    {
        var cut = _host.Context.RenderComponent<OfflineAuditLogs>();

        cut.Find("[data-live]").GetAttribute("data-live").Should().Be("offline");
        cut.Find("[data-live]").TextContent.Should().Contain("audit.realtime.offline");
    }

    private static void Raise(IRenderedComponent<TestAuditLogs> cut) =>
        cut.InvokeAsync(() => cut.Instance.RaiseAuditLogsChangedAsync()).GetAwaiter().GetResult();

    /// <summary>
    /// The page without a hub connection; tests raise "audit logs changed" and the refresh tick by hand.
    /// </summary>
    public sealed class TestAuditLogs : AuditLogs
    {
        public Task RaiseAuditLogsChangedAsync() => this.OnAuditLogsChangedAsync();

        public Task TickAsync() => this.OnAutoRefreshTickAsync();

        protected override Task StartHubAsync()
        {
            this.Live = LiveState.Live;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// The page when the first hub start fails.
    /// </summary>
    public sealed class OfflineAuditLogs : AuditLogs
    {
        protected override Task StartHubAsync()
        {
            this.Live = LiveState.Offline;
            return Task.CompletedTask;
        }
    }
}
