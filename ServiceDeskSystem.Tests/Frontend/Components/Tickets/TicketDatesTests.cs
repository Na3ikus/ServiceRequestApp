using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Time;
using ServiceDeskSystem.Components.Features.Tickets;

namespace ServiceDeskSystem.Tests.Frontend.Components.Tickets;

[TestFixture]
public class TicketDatesTests
{
    private static UserTimeZone Kyiv()
    {
        var zone = new UserTimeZone();
        zone.Set("Europe/Kyiv");
        return zone;
    }

    [Test]
    public void Stored_utc_is_shown_as_local_time_trimmed_to_minutes()
    {
        // EF returns stored UTC values as Unspecified.
        var stored = new DateTime(2026, 10, 10, 10, 12, 37, DateTimeKind.Unspecified);

        TicketDates.ToInput(stored, Kyiv()).Should().Be(new DateTime(2026, 10, 10, 13, 12, 0));
    }

    [Test]
    public void Picked_local_time_is_saved_as_utc()
    {
        TicketDates.ToStorage(new DateTime(2026, 10, 10, 13, 12, 0), Kyiv()).Should().Be(new DateTime(2026, 10, 10, 10, 12, 0, DateTimeKind.Utc));
    }

    [Test]
    public void Empty_values_stay_empty()
    {
        TicketDates.ToInput(null, Kyiv()).Should().BeNull();
        TicketDates.ToStorage(null, Kyiv()).Should().BeNull();
    }
}
