using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Time;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class UserTimeZoneTests
{
    [Test]
    public void Starts_in_the_server_zone()
    {
        new UserTimeZone().Zone.Should().Be(TimeZoneInfo.Local);
    }

    [Test]
    public void Converts_stored_utc_to_the_browser_zone()
    {
        var zone = new UserTimeZone();
        zone.Set("Europe/Kyiv");

        // EF hands back stored UTC values as Unspecified.
        var stored = new DateTime(2024, 4, 20, 8, 45, 0, DateTimeKind.Unspecified);

        zone.ToLocal(stored).Should().Be(new DateTime(2024, 4, 20, 11, 45, 0));
    }

    [Test]
    public void Raises_changed_only_for_a_new_valid_zone()
    {
        var zone = new UserTimeZone();
        var raised = 0;
        zone.Changed += (_, _) => raised++;

        zone.Set("America/New_York");
        zone.Set("America/New_York");
        zone.Set("Not/AZone");
        zone.Set(null);

        raised.Should().Be(TimeZoneInfo.Local.Id == "America/New_York" ? 0 : 1);
        zone.Zone.Id.Should().Be("America/New_York");
    }

    [Test]
    public void Converts_a_picked_local_time_to_utc()
    {
        var zone = new UserTimeZone();
        zone.Set("Europe/Kyiv");

        var utc = zone.ToUtc(new DateTime(2024, 4, 20, 11, 45, 0));

        utc.Should().Be(new DateTime(2024, 4, 20, 8, 45, 0));
        utc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Test]
    public void Local_to_utc_and_back_round_trips_across_seasons()
    {
        var zone = new UserTimeZone();
        zone.Set("America/New_York");

        foreach (var local in new[] { new DateTime(2026, 1, 15, 9, 30, 0), new DateTime(2026, 7, 15, 9, 30, 0) })
        {
            zone.ToLocal(zone.ToUtc(local)).Should().Be(local);
        }
    }

    [Test]
    public void A_time_inside_the_spring_forward_gap_moves_past_it()
    {
        var zone = new UserTimeZone();
        zone.Set("Europe/Kyiv");

        // 2026-03-29 03:00 local does not exist in Kyiv (clocks jump 03:00 -> 04:00, UTC+2 -> UTC+3).
        var utc = zone.ToUtc(new DateTime(2026, 3, 29, 3, 30, 0));

        utc.Should().Be(new DateTime(2026, 3, 29, 1, 30, 0));
    }
}
