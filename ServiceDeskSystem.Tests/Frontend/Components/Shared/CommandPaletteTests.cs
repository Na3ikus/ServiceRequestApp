using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Preferences;
using ServiceDeskSystem.Components.Shared;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

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
        cut.InvokeAsync(cut.Instance.OpenAsync);
        return cut;
    }

    private static List<string> Labels(IRenderedComponent<CommandPalette> cut) =>
        cut.FindAll("[role=option] .block.truncate:first-child").Select(e => e.TextContent.Trim()).ToList();

    private static IElement Option(IRenderedComponent<CommandPalette> cut, string label) =>
        cut.FindAll("[role=option]").Single(o => o.QuerySelector(".block")!.TextContent.Trim() == label);

    private static IElement Selected(IRenderedComponent<CommandPalette> cut) =>
        cut.FindAll("[role=option]").Single(o => o.GetAttribute("aria-selected") == "true");

    private IEnumerable<object?[]> Calls(string identifier) =>
        _host.Context.JSInterop.Invocations[identifier].Select(i => i.Arguments.ToArray());

    // commandPalette.js reports real pointer moves into a row as HoverItem(index).
    private static void Hover(IRenderedComponent<CommandPalette> cut, string label)
    {
        var index = Labels(cut).IndexOf(label);
        cut.InvokeAsync(() => cut.Instance.HoverItemAsync(index));
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
    public void Admin_sees_admin_and_statistics_commands_grouped()
    {
        var cut = RenderOpen();

        Labels(cut).Should().Equal(
            "cd ~", "cd ~/tickets", "cd ~/calendar", "cd ~/developer", "cd ~/statistics", "cd ~/profile", "cd ~/settings",
            "cd ~/admin", "cd ~/audit-logs", "new ticket",
            "theme light", "theme dark", "theme system",
            "accent lime", "accent amber", "accent phosphor", "accent ice",
            "lang en", "lang uk", "lang fr", "lang es", "lang de");
        cut.FindAll("[role=presentation]").Select(h => h.TextContent.Trim())
            .Should().Equal("# go", "# appearance", "# language");
        Option(cut, "new ticket").TextContent.Should().Contain("N");
        Option(cut, "cd ~/admin").TextContent.Should().Contain("cmd.desc.admin");
    }

    [Test]
    public void User_sees_own_tickets_but_not_statistics_or_admin()
    {
        _host.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = 2, Login = "user", Role = UserRole.User });

        var labels = Labels(RenderOpen());

        labels.Should().Contain("cd ~/my-tickets");
        labels.Should().NotContain(["cd ~/tickets", "cd ~/developer", "cd ~/statistics", "cd ~/admin", "cd ~/audit-logs"]);
    }

    [Test]
    public void Developer_sees_statistics_but_not_admin()
    {
        _host.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = 3, Login = "dev", Role = UserRole.Developer });

        var labels = Labels(RenderOpen());

        labels.Should().Contain(["cd ~/developer", "cd ~/statistics"]);
        labels.Should().NotContain(["cd ~/admin", "cd ~/audit-logs"]);
    }

    [Test]
    public void Every_word_of_the_query_must_match()
    {
        var cut = RenderOpen();

        cut.Find("input").Input("acc am");

        Labels(cut).Should().Equal("accent amber");
    }

    [Test]
    public void Headings_of_empty_groups_are_hidden_when_filtering()
    {
        var cut = RenderOpen();

        cut.Find("input").Input("lang");

        cut.FindAll("[role=presentation]").Select(h => h.TextContent.Trim()).Should().Equal("# language");
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
    public void Arrows_skip_group_headings()
    {
        var cut = RenderOpen();
        var goItems = Labels(cut).TakeWhile(l => l.StartsWith("cd ", StringComparison.Ordinal) || l == "new ticket").Count();

        for (var i = 0; i < goItems; i++)
        {
            cut.Find("input").KeyDown(key: "ArrowDown");
        }

        Selected(cut).TextContent.Should().Contain("theme light");
        cut.FindAll("[role=presentation][aria-selected]").Should().BeEmpty();
    }

    [Test]
    public void Current_theme_accent_and_language_are_marked()
    {
        _host.Preferences.SetupGet(p => p.Current).Returns(UserPreferences.Defaults with { Accent = "ice" });
        _host.Localization.SetupGet(l => l.CurrentLanguage).Returns("uk");

        var cut = RenderOpen();

        cut.FindAll("[role=option]").Where(o => o.QuerySelector("[data-current]") is not null)
            .Select(o => o.QuerySelector(".block")!.TextContent.Trim())
            .Should().Equal("theme light", "accent ice", "lang uk");
        Option(cut, "theme light").TextContent.Should().Contain("●");
    }

    [Test]
    public void Accent_rows_show_their_own_swatch()
    {
        var cut = RenderOpen();

        Option(cut, "accent amber").QuerySelector("[data-swatch]")!.GetAttribute("style").Should().Contain("#ffb000");
        Option(cut, "accent ice").QuerySelector("[data-swatch]")!.GetAttribute("style").Should().Contain("#4de8f0");
        Option(cut, "theme dark").QuerySelector("[data-swatch]").Should().BeNull();
    }

    [Test]
    public void Hovering_an_accent_previews_it_without_saving()
    {
        var cut = RenderOpen();

        Hover(cut, "accent amber");

        Calls("prefs.preview").Should().ContainSingle().Which.Should().Equal("accent", "amber");
        _host.Preferences.Verify(p => p.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Selecting_a_theme_with_the_keyboard_previews_it()
    {
        var cut = RenderOpen();
        cut.Find("input").Input("theme");

        cut.Find("input").KeyDown(key: "ArrowDown");

        Calls("prefs.preview").Select(a => a[1]).Should().Equal("light", "dark");
        _host.Theme.Verify(t => t.SetTheme(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Escape_restores_the_preview_and_closes()
    {
        var cut = RenderOpen();
        Hover(cut, "accent amber");

        cut.Find("input").KeyDown(key: "Escape");

        Calls("prefs.restore").Should().ContainSingle().Which.Should().Equal("accent");
        cut.FindAll("[role=dialog]").Should().BeEmpty();
    }

    [Test]
    public void Clicking_outside_restores_the_preview()
    {
        var cut = RenderOpen();
        Hover(cut, "theme dark");

        cut.Find("[role=dialog]").Click();

        Calls("prefs.restore").Should().ContainSingle().Which.Should().Equal("theme");
    }

    [Test]
    public void Moving_to_a_non_appearance_item_restores_the_preview()
    {
        var cut = RenderOpen();
        Hover(cut, "accent amber");

        Hover(cut, "lang fr");

        Calls("prefs.restore").Should().ContainSingle().Which.Should().Equal("accent");
        cut.FindAll("[role=dialog]").Should().NotBeEmpty();
    }

    [Test]
    public void Moving_between_accents_previews_without_restoring()
    {
        var cut = RenderOpen();
        Hover(cut, "accent amber");

        Hover(cut, "accent ice");

        Calls("prefs.preview").Select(a => a[1]).Should().Equal("amber", "ice");
        Calls("prefs.restore").Should().BeEmpty();
    }

    [Test]
    public void Enter_on_an_accent_saves_it_through_preferences()
    {
        var cut = RenderOpen();
        cut.Find("input").Input("acc am");

        cut.Find("input").KeyDown(key: "Enter");

        _host.Preferences.Verify(p => p.SetAsync(PreferenceSchema.Accent, "amber"), Times.Once);
        Calls("prefs.restore").Should().BeEmpty();
        cut.FindAll("[role=dialog]").Should().BeEmpty();
    }

    [Test]
    public void Clicking_a_theme_saves_it_through_the_theme_service()
    {
        var cut = RenderOpen();

        Option(cut, "theme dark").Click();

        _host.Theme.Verify(t => t.SetTheme("dark"), Times.Once);
        Calls("prefs.restore").Should().BeEmpty();
    }

    [Test]
    public void Lang_command_applies_and_stores_the_language()
    {
        var cut = RenderOpen();
        cut.Find("input").Input("lang uk");

        cut.Find("input").KeyDown(key: "Enter");

        _host.Localization.Verify(l => l.SetLanguage("uk"), Times.Once);
        Calls("localStorage.setItem").Should().ContainSingle().Which.Should().Equal("settings.language", "uk");
    }

    [Test]
    public void Saving_one_preference_restores_a_preview_of_the_other()
    {
        var cut = RenderOpen();
        Hover(cut, "accent amber");

        // Keyboard focus (Tab) moved to another row while the mouse preview stays.
        Option(cut, "theme dark").Click();

        Calls("prefs.restore").Should().ContainSingle().Which.Should().Equal("accent");
        _host.Theme.Verify(t => t.SetTheme("dark"), Times.Once);
        _host.Preferences.Verify(p => p.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Dispose_restores_an_active_preview()
    {
        var cut = RenderOpen();
        Hover(cut, "accent amber");

        cut.Instance.Dispose();

        Calls("prefs.restoreAll").Should().ContainSingle();
    }

    [Test]
    public void Dispose_without_preview_restores_nothing()
    {
        var cut = RenderOpen();

        cut.Instance.Dispose();

        Calls("prefs.restoreAll").Should().BeEmpty();
    }

    [Test]
    public void Hover_from_script_moves_the_selection()
    {
        var cut = RenderOpen();

        Hover(cut, "cd ~/calendar");

        Selected(cut).TextContent.Should().Contain("cd ~/calendar");
    }

    [Test]
    public void Input_is_a_combobox_pointing_at_the_selected_option()
    {
        var cut = RenderOpen();
        var input = cut.Find("input");

        input.GetAttribute("role").Should().Be("combobox");
        input.GetAttribute("aria-autocomplete").Should().Be("list");
        input.GetAttribute("aria-expanded").Should().Be("true");
        input.GetAttribute("aria-controls").Should().Be("cmd-list");
        cut.Find("[role=listbox]").Id.Should().Be("cmd-list");
        input.GetAttribute("aria-activedescendant").Should().Be(Selected(cut).Id);

        input.KeyDown(key: "ArrowDown");

        cut.Find("input").GetAttribute("aria-activedescendant").Should().Be("cmd-opt-1");
        Selected(cut).Id.Should().Be("cmd-opt-1");
        cut.FindAll("[role=option]").Should().OnlyContain(o => o.GetAttribute("tabindex") == "-1");
    }

    [Test]
    public void Combobox_is_collapsed_without_results()
    {
        var cut = RenderOpen();

        cut.Find("input").Input("zzzz-no-such-command");

        cut.Find("input").GetAttribute("aria-expanded").Should().Be("false");
        cut.Find("input").HasAttribute("aria-activedescendant").Should().BeFalse();
    }

    [Test]
    public void Escape_closes()
    {
        var cut = RenderOpen();

        cut.Find("input").KeyDown(key: "Escape");

        cut.FindAll("[role=dialog]").Should().BeEmpty();
    }
}
