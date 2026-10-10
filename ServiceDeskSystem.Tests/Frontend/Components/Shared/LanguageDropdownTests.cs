using Bunit;
using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Components.Shared;

namespace ServiceDeskSystem.Tests.Frontend.Components.Shared;

[TestFixture]
public class LanguageDropdownTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Trigger_shows_current_language_code()
    {
        var cut = _host.Context.RenderComponent<LanguageDropdown>();

        var trigger = cut.Find("button[aria-haspopup=menu]");
        trigger.TextContent.Should().Contain("en");
        trigger.GetAttribute("aria-expanded").Should().Be("false");
    }

    [Test]
    public void Opening_lists_all_supported_languages()
    {
        var cut = _host.Context.RenderComponent<LanguageDropdown>();

        cut.Find("button[aria-haspopup=menu]").Click();

        var rows = cut.FindAll("[role=menuitem]").ToList();
        rows.Should().HaveCount(_host.Localization.Object.SupportedLanguages.Count);
        cut.Find("button[aria-haspopup=menu]").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Test]
    public void Selected_row_has_aria_current_true()
    {
        var cut = _host.Context.RenderComponent<LanguageDropdown>();

        cut.Find("button[aria-haspopup=menu]").Click();

        var selected = cut.FindAll("[role=menuitem]").ToList()
            .Single(row => row.TextContent.Contains("English"));

        selected.GetAttribute("aria-current").Should().Be("true");
    }
}
