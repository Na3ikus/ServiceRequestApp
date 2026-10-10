using Bunit;
using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Components.UI.Primitives;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Primitives;

[TestFixture]
public class TagTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void StatusTag_new_is_accent_with_translated_text()
    {
        var cut = _host.Context.RenderComponent<StatusTag>(p => p.Add(x => x.Status, TicketStatus.New));

        var span = cut.Find("span");
        span.GetAttribute("data-tone").Should().Be("accent");
        span.TextContent.Should().Be("status.new");
    }

    [Test]
    public void PriorityTag_critical_is_danger()
    {
        var cut = _host.Context.RenderComponent<PriorityTag>(p => p.Add(x => x.Priority, TicketPriority.Critical));

        var span = cut.Find("span");
        span.GetAttribute("data-tone").Should().Be("danger");
        span.TextContent.Should().Be("priority.critical");
    }

    [Test]
    public void TypeTag_bug_is_active()
    {
        var cut = _host.Context.RenderComponent<TypeTag>(p => p.Add(x => x.Type, TicketType.Bug));

        var span = cut.Find("span");
        span.GetAttribute("data-tone").Should().Be("active");
        span.TextContent.Should().Be("ticketType.bug");
    }

    [Test]
    public void RoleTag_developer_is_muted()
    {
        var cut = _host.Context.RenderComponent<RoleTag>(p => p.Add(x => x.Role, UserRole.Developer));

        var span = cut.Find("span");
        span.GetAttribute("data-tone").Should().Be("muted");
        span.TextContent.Should().Be("admin.roleDeveloper");
    }

    [Test]
    public void Accent_tone_classes()
    {
        var cut = _host.Context.RenderComponent<Tag>(p => p
            .Add(x => x.Tone, Tone.Accent)
            .AddChildContent("X"));

        var span = cut.Find("span");
        var @class = span.GetAttribute("class");
        @class.Should().Contain("bg-accent");
        @class.Should().Contain("text-on-accent");
        @class.Should().Contain("border-black");
        @class.Should().Contain("dark:text-accent");
        @class.Should().Contain("dark:bg-transparent");
    }

    [Test]
    public void Tag_title_renders_attribute()
    {
        var cut = _host.Context.RenderComponent<Tag>(p => p
            .Add(x => x.Title, "x")
            .AddChildContent("X"));

        cut.Find("span").GetAttribute("title").Should().Be("x");
    }
}
