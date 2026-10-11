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
}
