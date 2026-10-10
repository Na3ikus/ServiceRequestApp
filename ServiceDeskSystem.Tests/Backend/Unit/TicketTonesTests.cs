using System;
using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Components.UI.Primitives;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class TicketTonesTests
{
    [TestCase(TicketStatus.New, Tone.Accent)]
    [TestCase(TicketStatus.Open, Tone.Active)]
    [TestCase(TicketStatus.InProgress, Tone.Active)]
    [TestCase(TicketStatus.Testing, Tone.Active)]
    [TestCase(TicketStatus.CodeReview, Tone.Active)]
    [TestCase(TicketStatus.Resolved, Tone.Muted)]
    [TestCase(TicketStatus.Done, Tone.Muted)]
    [TestCase(TicketStatus.Closed, Tone.Muted)]
    public void For_maps_status_to_tone(TicketStatus status, Tone expected)
    {
        TicketTones.For(status).Should().Be(expected);
    }

    [TestCase(TicketPriority.Critical, Tone.Danger)]
    [TestCase(TicketPriority.High, Tone.Active)]
    [TestCase(TicketPriority.Medium, Tone.Muted)]
    [TestCase(TicketPriority.Low, Tone.Muted)]
    public void For_maps_priority_to_tone(TicketPriority priority, Tone expected)
    {
        TicketTones.For(priority).Should().Be(expected);
    }

    [TestCase(TicketType.Bug, Tone.Active)]
    [TestCase(TicketType.Support, Tone.Muted)]
    [TestCase(TicketType.Project, Tone.Muted)]
    [TestCase(TicketType.Consultation, Tone.Muted)]
    public void For_maps_type_to_tone(TicketType type, Tone expected)
    {
        TicketTones.For(type).Should().Be(expected);
    }

    [Test]
    public void For_covers_every_status_enum_value()
    {
        foreach (var value in Enum.GetValues<TicketStatus>())
        {
            FluentActions.Invoking(() => TicketTones.For(value)).Should().NotThrow();
        }
    }

    [Test]
    public void For_covers_every_priority_enum_value()
    {
        foreach (var value in Enum.GetValues<TicketPriority>())
        {
            FluentActions.Invoking(() => TicketTones.For(value)).Should().NotThrow();
        }
    }

    [Test]
    public void For_covers_every_type_enum_value()
    {
        foreach (var value in Enum.GetValues<TicketType>())
        {
            FluentActions.Invoking(() => TicketTones.For(value)).Should().NotThrow();
        }
    }
}
