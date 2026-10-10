using FluentAssertions;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Theme;

namespace ServiceDeskSystem.Tests.Backend.Unit;

public class ThemeServiceTests
{
    private Mock<IJSRuntime> _js = null!;

    [SetUp]
    public void SetUp()
    {
        _js = new Mock<IJSRuntime>();
    }

    private async Task<ThemeService> CreateInitializedAsync(string preferred, string resolved)
    {
        _js.Setup(j => j.InvokeAsync<string>("prefs.get", It.Is<object?[]?>(a => a != null && (string)a[0]! == "theme")))
            .ReturnsAsync(preferred);
        _js.Setup(j => j.InvokeAsync<string>("prefs.resolveTheme", It.IsAny<object?[]?>()))
            .ReturnsAsync(resolved);

        var service = new ThemeService(_js.Object);
        await service.InitializeAsync();
        return service;
    }

    [Test]
    public async Task OnSystemThemeChanged_UpdatesIsDarkModeWhenPreferenceIsSystem()
    {
        using var service = await CreateInitializedAsync("system", "light");
        var raised = 0;
        service.ThemeChanged += (_, _) => raised++;

        service.OnSystemThemeChanged(true);

        service.IsDarkMode.Should().BeTrue();
        raised.Should().Be(1);
        _js.Verify(j => j.InvokeAsync<IJSVoidResult>("prefs.onSystemThemeChange", It.IsAny<object?[]?>()), Times.Once);
    }

    [Test]
    public async Task OnSystemThemeChanged_IgnoredWhenPreferenceIsExplicitLight()
    {
        using var service = await CreateInitializedAsync("light", "light");
        var raised = 0;
        service.ThemeChanged += (_, _) => raised++;

        service.OnSystemThemeChanged(true);

        service.IsDarkMode.Should().BeFalse();
        raised.Should().Be(0);
    }

    [Test]
    public async Task SetTheme_CallsPrefsSetWithTheme()
    {
        using var service = await CreateInitializedAsync("light", "light");

        service.SetTheme("dark");

        _js.Verify(j => j.InvokeAsync<IJSVoidResult>(
            "prefs.set",
            It.Is<object?[]?>(a => a != null && a.Length == 2 && (string)a[0]! == "theme" && (string)a[1]! == "dark")),
            Times.Once);
        service.IsDarkMode.Should().BeTrue();
    }
}
