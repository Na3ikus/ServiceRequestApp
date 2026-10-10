using Bunit;
using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Components.Shared;

namespace ServiceDeskSystem.Tests.Frontend.Components.Shared;

[TestFixture]
public class CommandPaletteTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    private IRenderedComponent<CommandPalette> RenderOpen()
    {
        var cut = _host.Context.RenderComponent<CommandPalette>();
        cut.InvokeAsync(cut.Instance.Open);
        return cut;
    }

    [Test]
    public void Opens_with_prompt_and_items_without_icons()
    {
        var cut = RenderOpen();

        cut.Find("[role=dialog]").TextContent.Should().Contain(">");
        cut.FindAll("[role=option]").Should().NotBeEmpty();
        cut.FindAll("svg").Should().BeEmpty();
    }

    [Test]
    public void First_item_is_selected_with_marker_and_arrows_move_it()
    {
        var cut = RenderOpen();

        var options = cut.FindAll("[role=option]").ToList();
        options[0].GetAttribute("aria-selected").Should().Be("true");
        options[0].TextContent.Should().Contain("›");

        cut.Find("input").KeyDown(key: "ArrowDown");

        options = cut.FindAll("[role=option]").ToList();
        options[0].GetAttribute("aria-selected").Should().Be("false");
        options[1].GetAttribute("aria-selected").Should().Be("true");
    }

    [Test]
    public void No_match_shows_empty_state()
    {
        var cut = RenderOpen();

        cut.Find("input").Input("zzzz-no-such-command");

        cut.FindAll("[role=option]").Should().BeEmpty();
        cut.Markup.Should().Contain("cmd.noResults");
    }

    [Test]
    public void Escape_closes()
    {
        var cut = RenderOpen();

        cut.Find("input").KeyDown(key: "Escape");

        cut.FindAll("[role=dialog]").Should().BeEmpty();
    }
}
