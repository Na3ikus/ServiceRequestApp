using System.Net;
using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Preferences;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using SettingsPage = ServiceDeskSystem.Components.Pages.Settings.Settings;

namespace ServiceDeskSystem.Tests.Frontend.Components.Settings;

[TestFixture]
public class SettingsPageTests
{
    private ComponentTestHost _host;
    private Mock<IEmailSender> _email;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();

        var httpFactory = new Mock<IHttpClientFactory>();
        httpFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(new StubHandler(HttpStatusCode.OK)));
        _email = new Mock<IEmailSender>();
        _email.Setup(e => e.CheckConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "SMTP OK"));

        _host.Context.Services.AddSingleton(httpFactory.Object);
        _host.Context.Services.AddSingleton(_email.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private IRenderedComponent<SettingsPage> Render() => _host.Context.RenderComponent<SettingsPage>();

    private static IElement Option(IRenderedComponent<SettingsPage> cut, string group, string text) =>
        cut.FindAll($"[role=radiogroup][aria-label='{group}'] button[role=radio]")
            .ToList()
            .Single(b => b.TextContent.Trim() == text);

    private static IElement ButtonWithText(IRenderedComponent<SettingsPage> cut, string text) =>
        cut.FindAll("button").ToList().First(b => b.TextContent.Contains(text, StringComparison.Ordinal));

    [Test]
    public void Has_no_sidebar_setting()
    {
        var cut = Render();

        cut.FindAll("[role=radiogroup][aria-label='settings.sidebar']").Should().BeEmpty();
        cut.Markup.Should().NotContain("Ctrl+B");
    }

    [Test]
    public void Heading_is_path_header()
    {
        var cut = Render();

        cut.Find("h1").TextContent.Should().Be("~/settings");
        cut.Find("[aria-current=page]").TextContent.Should().Be("settings");
    }

    [Test]
    public void RendersAllSectionsForAdmin()
    {
        var cut = Render();

        var headings = cut.FindAll("h2").Select(h => h.TextContent.Trim()).ToList();
        foreach (var section in new[] { "appearance", "layout", "locale", "tickets", "notifications", "keyboard" })
        {
            headings.Should().Contain($"# settings.section.{section}");
        }

        headings.Should().Contain(h => h.StartsWith("# settings.section.system", StringComparison.Ordinal));
    }

    [Test]
    public void HidesSystemSectionForNonAdmin()
    {
        _host.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = 2, Login = "user", Role = UserRole.User });

        var cut = Render();

        cut.FindAll("h2").Select(h => h.TextContent.Trim())
            .Should().NotContain(h => h.StartsWith("# settings.section.system", StringComparison.Ordinal));
        _email.Verify(e => e.CheckConnectionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void ClickingCompactDensityCallsSetAsync()
    {
        var cut = Render();

        Option(cut, "settings.density", "compact").Click();

        _host.Preferences.Verify(p => p.SetAsync("density", "compact"), Times.Once);
    }

    [Test]
    public void ClickingAmberAccentCallsSetAsync()
    {
        var cut = Render();

        cut.Find("button[aria-label='settings.accent.amber']").Click();

        _host.Preferences.Verify(p => p.SetAsync("accent", "amber"), Times.Once);
    }

    [Test]
    public void ClickingDarkThemeCallsSetTheme()
    {
        var cut = Render();

        Option(cut, "settings.theme", "dark").Click();

        _host.Theme.Verify(t => t.SetTheme("dark"), Times.Once);
    }

    [Test]
    public void DesktopToggleRevertsWhenPermissionDenied()
    {
        _host.Context.JSInterop.Setup<string>("notificationManager.requestDesktopPermission").SetResult("denied");
        var cut = Render();

        cut.Find("button[role=switch][aria-label='settings.desktopNotifications']").Click();

        _host.Preferences.Verify(p => p.SetAsync("desktopNotifications", It.IsAny<string>()), Times.Never);
        cut.Find("button[role=switch][aria-label='settings.desktopNotifications']")
            .GetAttribute("aria-checked").Should().Be("false");
        cut.Markup.Should().Contain("settings.desktopDenied");
    }

    [Test]
    public void DesktopToggleEnablesWhenPermissionGranted()
    {
        _host.Context.JSInterop.Setup<string>("notificationManager.requestDesktopPermission").SetResult("granted");
        var cut = Render();

        cut.Find("button[role=switch][aria-label='settings.desktopNotifications']").Click();

        _host.Preferences.Verify(p => p.SetAsync("desktopNotifications", "true"), Times.Once);
    }

    [Test]
    public void ResetAsksForConfirmationBeforeCallingResetAsync()
    {
        var cut = Render();

        ButtonWithText(cut, "settings.resetDefaults").Click();
        _host.Preferences.Verify(p => p.ResetAsync(), Times.Never);

        cut.Find("button[data-modal-save]").Click();

        _host.Preferences.Verify(p => p.ResetAsync(), Times.Once);
        _host.Theme.Verify(t => t.SetTheme("system"), Times.Once);
    }

    [Test]
    public void RendersDefaultsWhenLoadAsyncReturnsDefaults()
    {
        var cut = Render();

        Option(cut, "settings.density", "comfortable").GetAttribute("aria-checked").Should().Be("true");
        Option(cut, "settings.scale", "100%").GetAttribute("aria-checked").Should().Be("true");
        cut.Find("button[aria-label='settings.accent.lime']").GetAttribute("aria-checked").Should().Be("true");
        cut.Find("button[role=switch][aria-label='settings.sound']").GetAttribute("aria-checked").Should().Be("true");
    }

    private sealed class StubHandler(HttpStatusCode status) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(status));
    }
}
