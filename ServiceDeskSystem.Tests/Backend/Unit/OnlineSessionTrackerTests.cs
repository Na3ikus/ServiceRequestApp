using System.Net;
using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Services.Presence;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class OnlineSessionTrackerTests
{
    private OnlineSessionTracker _tracker;
    private int _changes;

    [SetUp]
    public void SetUp()
    {
        _tracker = new OnlineSessionTracker(TimeProvider.System);
        _changes = 0;
        _tracker.Changed += (_, _) => _changes++;
    }

    [Test]
    public void A_new_tab_is_a_connected_guest_with_its_ip()
    {
        _tracker.Open("c1", "10.0.0.5");

        _tracker.Sessions.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            CircuitId = "c1",
            IpAddress = "10.0.0.5",
            UserId = (int?)null,
            Login = (string?)null,
            Role = (UserRole?)null,
            IsConnected = true,
        });
        _changes.Should().Be(1);
    }

    [Test]
    public void Signing_in_and_out_updates_the_tab()
    {
        _tracker.Open("c1", "10.0.0.5");

        _tracker.SetUser("c1", new User { Id = 2, Login = "dev", Role = UserRole.Developer });
        _tracker.Sessions.Single().Should().BeEquivalentTo(new { UserId = 2, Login = "dev", Role = UserRole.Developer });

        _tracker.SetUser("c1", null);
        _tracker.Sessions.Single().Login.Should().BeNull();
        _changes.Should().Be(3);
    }

    [Test]
    public void Nothing_is_raised_when_nothing_changed()
    {
        _tracker.Open("c1", "10.0.0.5");

        _tracker.SetConnected("c1", true);
        _tracker.SetUser("c1", null);
        _tracker.SetUser("gone", new User { Id = 1, Login = "x" });
        _tracker.Close("gone");

        _changes.Should().Be(1);
    }

    [Test]
    public void A_dropped_connection_shows_until_it_returns_or_the_tab_closes()
    {
        _tracker.Open("c1", "10.0.0.5");

        _tracker.SetConnected("c1", false);
        _tracker.Sessions.Single().IsConnected.Should().BeFalse();

        _tracker.SetConnected("c1", true);
        _tracker.Sessions.Single().IsConnected.Should().BeTrue();

        _tracker.Close("c1");
        _tracker.Sessions.Should().BeEmpty();
    }

    [Test]
    public void Signed_in_tabs_come_first_by_login_then_guests()
    {
        _tracker.Open("guest", "1.1.1.1");
        _tracker.Open("zed", "1.1.1.2");
        _tracker.SetUser("zed", new User { Id = 9, Login = "zed" });
        _tracker.Open("anna", "1.1.1.3");
        _tracker.SetUser("anna", new User { Id = 4, Login = "Anna" });

        _tracker.Sessions.Select(s => s.CircuitId).Should().Equal("anna", "zed", "guest");
    }

    [Test]
    public void A_failing_listener_does_not_stop_the_change()
    {
        _tracker.Changed += (_, _) => throw new ObjectDisposedException("circuit");

        _tracker.Open("c1", "10.0.0.5");

        _tracker.Sessions.Should().ContainSingle();
        _changes.Should().Be(1);
    }

    [TestCase("::ffff:192.168.1.20", "192.168.1.20")]
    [TestCase("::1", "::1")]
    [TestCase("10.0.0.5", "10.0.0.5")]
    public void Ips_are_shown_the_way_people_read_them(string raw, string shown)
    {
        OnlineCircuitHandler.FormatIp(IPAddress.Parse(raw)).Should().Be(shown);
    }
}
