using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;
using ServiceDeskSystem.Components.Pages.Settings.Controls;
using ServiceDeskSystem.Components.UI.Primitives;

namespace ServiceDeskSystem.Tests.Frontend.Components.Settings;

[TestFixture]
public class SettingsControlsTests
{
    private static readonly IReadOnlyList<(string Value, string Text)> DensityOptions =
    [
        ("comfortable", "comfortable"),
        ("compact", "compact"),
        ("dense", "dense"),
    ];

    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    private IRenderedComponent<SegmentedControl> RenderSegmented(string value, Action<string> onChanged) =>
        _host.Context.RenderComponent<SegmentedControl>(p => p
            .Add(c => c.Label, "Density")
            .Add(c => c.Options, DensityOptions)
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, onChanged)));

    [Test]
    public void SegmentedControl_MarksSelectedOption()
    {
        var cut = RenderSegmented("compact", _ => { });

        cut.Find("[role=radiogroup]").GetAttribute("aria-label").Should().Be("Density");
        var radios = cut.FindAll("button[role=radio]").ToList();
        radios.Should().HaveCount(3);
        radios[0].GetAttribute("aria-checked").Should().Be("false");
        radios[1].GetAttribute("aria-checked").Should().Be("true");
        radios[2].GetAttribute("aria-checked").Should().Be("false");
        radios[1].GetAttribute("tabindex").Should().Be("0");
        radios[0].GetAttribute("tabindex").Should().Be("-1");
    }

    [Test]
    public void SegmentedControl_ClickRaisesValueChanged()
    {
        string? changed = null;
        var cut = RenderSegmented("comfortable", v => changed = v);

        cut.FindAll("button[role=radio]").ToList()[1].Click();

        changed.Should().Be("compact");
    }

    [Test]
    public void SegmentedControl_ArrowRightSelectsNextAndWrapsAtEnd()
    {
        string? changed = null;
        var cut = RenderSegmented("comfortable", v => changed = v);

        cut.FindAll("button[role=radio]").ToList()[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        changed.Should().Be("compact");

        cut.SetParametersAndRender(p => p.Add(c => c.Value, "dense"));
        cut.FindAll("button[role=radio]").ToList()[2].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        changed.Should().Be("comfortable");

        cut.FindAll("button[role=radio]").ToList()[2].KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });
        changed.Should().Be("compact");
    }

    [Test]
    public void ToggleSwitch_RendersAriaCheckedFalseExplicitly()
    {
        var cut = _host.Context.RenderComponent<ToggleSwitch>(p => p
            .Add(c => c.Label, "Sound")
            .Add(c => c.Value, false));

        var button = cut.Find("button[role=switch]");
        button.GetAttribute("aria-checked").Should().Be("false");
        button.GetAttribute("aria-label").Should().Be("Sound");
    }

    [Test]
    public void ToggleSwitch_ClickRaisesValueChangedTrue()
    {
        bool? changed = null;
        var cut = _host.Context.RenderComponent<ToggleSwitch>(p => p
            .Add(c => c.Label, "Sound")
            .Add(c => c.Value, false)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<bool>(this, v => changed = v)));

        cut.Find("button[role=switch]").Click();

        changed.Should().BeTrue();
    }

    [Test]
    public void ToggleSwitch_DisabledDoesNotRaise()
    {
        var raised = false;
        var cut = _host.Context.RenderComponent<ToggleSwitch>(p => p
            .Add(c => c.Label, "Sound")
            .Add(c => c.Value, false)
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<bool>(this, _ => raised = true)));

        cut.Find("button[role=switch]").Click();

        raised.Should().BeFalse();
    }

    [Test]
    public void AccentPicker_RendersFourRadiosWithLabels()
    {
        var cut = _host.Context.RenderComponent<AccentPicker>(p => p.Add(c => c.Value, "lime"));

        var radios = cut.FindAll("button[role=radio]").ToList();
        radios.Select(r => r.GetAttribute("aria-label")).Should().Equal(
            "settings.accent.lime", "settings.accent.amber", "settings.accent.phosphor", "settings.accent.ice");
        radios[0].GetAttribute("aria-checked").Should().Be("true");
        radios[1].GetAttribute("aria-checked").Should().Be("false");
    }

    [Test]
    public void AccentPicker_ClickAmberRaisesValueChanged()
    {
        string? changed = null;
        var cut = _host.Context.RenderComponent<AccentPicker>(p => p
            .Add(c => c.Value, "lime")
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => changed = v)));

        cut.Find("button[aria-label='settings.accent.amber']").Click();

        changed.Should().Be("amber");
    }

    [Test]
    public void SettingRow_RendersLabelDescriptionAndContent()
    {
        var cut = _host.Context.RenderComponent<SettingRow>(p => p
            .Add(c => c.Label, "Density")
            .Add(c => c.Description, "Row spacing")
            .AddChildContent("<span class=\"ctl\">x</span>"));

        cut.Markup.Should().Contain("Density").And.Contain("Row spacing");
        cut.Find("span.ctl").TextContent.Should().Be("x");
    }
}
