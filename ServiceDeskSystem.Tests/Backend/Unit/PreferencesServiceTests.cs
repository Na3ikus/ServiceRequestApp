using FluentAssertions;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Preferences;

namespace ServiceDeskSystem.Tests.Backend.Unit;

public class PreferencesServiceTests
{
    private Mock<IJSRuntime> _js = null!;

    [SetUp]
    public void SetUp()
    {
        _js = new Mock<IJSRuntime>();
    }

    private void SetupGetAll(Dictionary<string, string> values)
    {
        _js.Setup(j => j.InvokeAsync<Dictionary<string, string>>("prefs.getAll", It.IsAny<object?[]?>()))
            .ReturnsAsync(values);
    }

    private PreferencesService CreateService() => new(_js.Object);

    [Test]
    public async Task LoadAsync_MapsJsValues()
    {
        SetupGetAll(new Dictionary<string, string>
        {
            ["theme"] = "dark",
            ["accent"] = "amber",
            ["scale"] = "110",
            ["contrast"] = "high",
            ["reducedMotion"] = "true",
            ["density"] = "compact",
            ["contentWidth"] = "wide",
            ["dateFormat"] = "YYYY-MM-DD",
            ["defaultView"] = "KANBAN",
            ["sound"] = "false",
            ["desktopNotifications"] = "true",
            ["toastPosition"] = "bottom-center",
            ["shortcuts"] = "false",
        });

        var result = await CreateService().LoadAsync();

        result.Should().Be(new UserPreferences(
            "amber", 110, true, true, "compact", "wide", "YYYY-MM-DD", "KANBAN", false, true, "bottom-center", false));
    }

    [Test]
    public async Task LoadAsync_FallsBackPerFieldOnInvalidValues()
    {
        SetupGetAll(new Dictionary<string, string>
        {
            ["accent"] = "custom",
            ["scale"] = "abc",
            ["density"] = "compact",
        });

        var result = await CreateService().LoadAsync();

        result.Accent.Should().Be("lime");
        result.Scale.Should().Be(100);
        result.Density.Should().Be("compact");
        result.Sound.Should().BeTrue();
    }

    [Test]
    public async Task LoadAsync_ReturnsDefaultsWhenJsThrows()
    {
        _js.Setup(j => j.InvokeAsync<Dictionary<string, string>>("prefs.getAll", It.IsAny<object?[]?>()))
            .ThrowsAsync(new InvalidOperationException("prerendering"));

        var service = CreateService();
        var result = await service.LoadAsync();

        result.Should().Be(UserPreferences.Defaults);
        service.Current.Should().Be(UserPreferences.Defaults);

        _js.Setup(j => j.InvokeAsync<Dictionary<string, string>>("prefs.getAll", It.IsAny<object?[]?>()))
            .ThrowsAsync(new JSException("boom"));
        (await service.LoadAsync()).Should().Be(UserPreferences.Defaults);
    }

    [Test]
    public async Task SetAsync_RejectsUnknownName()
    {
        var act = () => CreateService().SetAsync("theme", "dark");

        await act.Should().ThrowAsync<ArgumentException>();
        _js.Verify(j => j.InvokeAsync<IJSVoidResult>("prefs.set", It.IsAny<object?[]?>()), Times.Never);
    }

    [Test]
    public async Task SetAsync_RejectsDisallowedValue()
    {
        var act = () => CreateService().SetAsync(PreferenceSchema.Density, "huge");

        await act.Should().ThrowAsync<ArgumentException>();
        _js.Verify(j => j.InvokeAsync<IJSVoidResult>("prefs.set", It.IsAny<object?[]?>()), Times.Never);
    }

    [Test]
    public async Task SetAsync_UpdatesCurrentAndRaisesChanged()
    {
        var service = CreateService();
        var raised = 0;
        service.Changed += (_, _) => raised++;

        await service.SetAsync(PreferenceSchema.Density, "compact");

        service.Current.Density.Should().Be("compact");
        raised.Should().Be(1);
        _js.Verify(j => j.InvokeAsync<IJSVoidResult>(
            "prefs.set",
            It.Is<object?[]?>(a => a != null && a.Length == 2 && (string)a[0]! == "density" && (string)a[1]! == "compact")),
            Times.Once);
    }

    [Test]
    public async Task ResetAsync_RestoresDefaultsAndRaisesChanged()
    {
        var service = CreateService();
        await service.SetAsync(PreferenceSchema.Accent, "ice");
        var raised = 0;
        service.Changed += (_, _) => raised++;

        await service.ResetAsync();

        service.Current.Should().Be(UserPreferences.Defaults);
        raised.Should().Be(1);
        _js.Verify(j => j.InvokeAsync<IJSVoidResult>("prefs.reset", It.IsAny<object?[]?>()), Times.Once);
    }

    [TestCase("DD/MM/YYYY", "09/10/2026")]
    [TestCase("MM/DD/YYYY", "10/09/2026")]
    [TestCase("YYYY-MM-DD", "2026-10-09")]
    public async Task FormatDate_UsesDateFormatPreference(string format, string expected)
    {
        var service = CreateService();
        await service.SetAsync(PreferenceSchema.DateFormat, format);

        service.FormatDate(new DateTime(2026, 10, 9)).Should().Be(expected);
    }
}
