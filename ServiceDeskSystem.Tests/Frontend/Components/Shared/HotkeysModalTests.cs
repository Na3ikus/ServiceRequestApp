using Bunit;
using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Components.Shared;

namespace ServiceDeskSystem.Tests.Frontend.Components.Shared;

[TestFixture]
public class HotkeysModalTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Hidden_until_shown()
    {
        var cut = _host.Context.RenderComponent<HotkeysModal>();

        cut.FindAll("[role=dialog]").Should().BeEmpty();
    }

    [Test]
    public void Shows_hotkey_rows_with_kbd_and_no_sidebar_row()
    {
        var cut = _host.Context.RenderComponent<HotkeysModal>();

        cut.InvokeAsync(cut.Instance.Show);

        cut.Find(".amodal-title").TextContent.Should().Be("settings.hotkeysTitle");
        cut.FindAll("li").Should().HaveCount(5);
        cut.FindAll("kbd").Select(k => k.TextContent).Should().Equal("?", "H", "N", "T", "/", "D");
        cut.Markup.Should().NotContain("hotkeySidebar");
        cut.FindAll("svg").Should().BeEmpty();
    }

    [Test]
    public void Close_button_hides_it()
    {
        var cut = _host.Context.RenderComponent<HotkeysModal>();
        cut.InvokeAsync(cut.Instance.Show);

        cut.Find(".amodal-footer button").Click();

        cut.FindAll("[role=dialog]").Should().BeEmpty();
    }
}
