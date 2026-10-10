using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using ServiceDeskSystem.Components.UI.Primitives;

namespace ServiceDeskSystem.Tests.Frontend.Components.Primitives;

[TestFixture]
public class LayoutPrimitivesTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void PageHeader_links_segments_with_href()
    {
        var segments = new[]
        {
            new PathSegment("tickets", "/tickets"),
            new PathSegment("1042"),
        };

        var cut = _host.Context.RenderComponent<PageHeader>(p => p
            .Add(x => x.Segments, segments));

        var link = cut.Find("a[href='/tickets']");
        link.TextContent.Should().Be("tickets");

        var current = cut.Find("[aria-current=page]");
        current.TextContent.Should().Be("1042");

        cut.Find("h1").TextContent.Should().StartWith("~/");
    }

    [Test]
    public void PageHeader_renders_subtitle_and_actions()
    {
        var segments = new[] { new PathSegment("settings") };

        var cut = _host.Context.RenderComponent<PageHeader>(p => p
            .Add(x => x.Segments, segments)
            .Add(x => x.Subtitle, "manage your preferences")
            .Add(x => x.Actions, (RenderFragment)(builder => builder.AddContent(0, "action-slot"))));

        cut.Markup.Should().Contain("manage your preferences");
        cut.Markup.Should().Contain("action-slot");
    }

    [Test]
    public void Panel_header_only_with_title_or_meta()
    {
        var withoutHeader = _host.Context.RenderComponent<Panel>();
        withoutHeader.FindAll(".border-b").ToList().Should().BeEmpty();

        var withTitle = _host.Context.RenderComponent<Panel>(p => p
            .Add(x => x.Title, "details"));
        withTitle.FindAll(".border-b").ToList().Should().HaveCount(1);
        withTitle.Markup.Should().Contain("# details");
    }

    [Test]
    public void EmptyState_action_invokes_callback()
    {
        var invoked = 0;

        var cut = _host.Context.RenderComponent<EmptyState>(p => p
            .Add(x => x.Message, "no tickets")
            .Add(x => x.ActionText, "create one")
            .Add(x => x.OnAction, () => invoked++));

        cut.Find("button").Click();

        invoked.Should().Be(1);
    }

    [Test]
    public void EmptyState_without_action_has_no_button()
    {
        var cut = _host.Context.RenderComponent<EmptyState>(p => p
            .Add(x => x.Message, "no tickets"));

        cut.FindAll("button").ToList().Should().BeEmpty();
        cut.Markup.Should().Contain("&gt; no tickets");
    }

    [Test]
    public void EmptyState_hotkey_renders_kbd()
    {
        var cut = _host.Context.RenderComponent<EmptyState>(p => p
            .Add(x => x.Message, "no tickets")
            .Add(x => x.Hotkey, "N"));

        cut.Find("kbd").TextContent.Should().Be("N");
    }
}
