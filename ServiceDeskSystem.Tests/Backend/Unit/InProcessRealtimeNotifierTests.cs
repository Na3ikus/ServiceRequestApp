using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Realtime;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class InProcessRealtimeNotifierTests
{
    private InProcessRealtimeNotifier _notifier;

    [SetUp]
    public void SetUp() => _notifier = new InProcessRealtimeNotifier();

    [Test]
    public async Task Raises_each_kind_of_change_to_its_own_listeners()
    {
        var tickets = 0;
        var audit = 0;
        _notifier.TicketsChanged += (_, _) => tickets++;
        _notifier.AuditLogsChanged += (_, _) => audit++;

        await _notifier.NotifyTicketsChangedAsync();
        await _notifier.NotifyTicketsChangedAsync();
        await _notifier.NotifyAuditLogsChangedAsync();

        tickets.Should().Be(2);
        audit.Should().Be(1);
    }

    [Test]
    public async Task Notification_changes_carry_distinct_real_user_ids()
    {
        IReadOnlyCollection<int>? received = null;
        _notifier.NotificationsChanged += (_, e) => received = e.UserIds;

        await _notifier.NotifyNotificationsChangedAsync([3, 0, 3, -1, 5]);

        received.Should().Equal(3, 5);
    }

    [Test]
    public async Task No_event_when_there_is_no_real_user()
    {
        var raised = false;
        _notifier.NotificationsChanged += (_, _) => raised = true;

        await _notifier.NotifyNotificationsChangedAsync([0, -2]);

        raised.Should().BeFalse();
    }

    [Test]
    public async Task A_failing_listener_neither_fails_the_change_nor_silences_the_others()
    {
        var heard = false;
        _notifier.TicketsChanged += (_, _) => throw new ObjectDisposedException("circuit");
        _notifier.TicketsChanged += (_, _) => heard = true;

        var act = () => _notifier.NotifyTicketsChangedAsync();

        await act.Should().NotThrowAsync();
        heard.Should().BeTrue();
    }
}
