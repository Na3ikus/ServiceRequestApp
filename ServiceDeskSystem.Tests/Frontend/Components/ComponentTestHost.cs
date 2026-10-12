using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Localization;
using ServiceDeskSystem.Application.Services.Preferences;
using ServiceDeskSystem.Application.Services.Sla;
using ServiceDeskSystem.Application.Services.Theme;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using BunitContext = Bunit.TestContext;

namespace ServiceDeskSystem.Tests.Frontend.Components;

/// <summary>
/// Wires up the services every <c>BaseComponent</c> descendant injects, so component
/// tests only have to describe the case under test.
/// </summary>
public sealed class ComponentTestHost : IDisposable
{
    private bool disposed;

    public ComponentTestHost()
    {
        this.Context = new BunitContext();
        this.Context.JSInterop.Mode = JSRuntimeMode.Loose;

        this.Localization = new Mock<ILocalizationService>();
        this.Localization.Setup(l => l.Translate(It.IsAny<string>())).Returns<string>(key => key);
        this.Localization.SetupGet(l => l.CurrentLanguage).Returns("en");
        this.Localization.SetupGet(l => l.SupportedLanguages).Returns(LocalizationConstants.SupportedLanguages);

        this.Theme = new Mock<IThemeService>();
        this.Theme.SetupGet(t => t.IsDarkMode).Returns(false);
        this.Theme.SetupGet(t => t.CurrentTheme).Returns("light");

        this.Auth = new Mock<IAuthService>();
        this.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = 1, Login = "admin", Role = UserRole.Admin });

        this.SlaPolicyService = new Mock<ISlaPolicyService>();
        this.ToastService = new Mock<IToastService>();

        this.Preferences = new Mock<IPreferencesService>();
        this.Preferences.SetupGet(p => p.Current).Returns(UserPreferences.Defaults);
        this.Preferences.Setup(p => p.LoadAsync()).ReturnsAsync(UserPreferences.Defaults);

        this.Context.Services.AddSingleton(this.Preferences.Object);
        this.Context.Services.AddSingleton(this.Localization.Object);
        this.Context.Services.AddSingleton(this.Theme.Object);
        this.Context.Services.AddSingleton(this.Auth.Object);
        this.Context.Services.AddSingleton(this.SlaPolicyService.Object);
        this.Context.Services.AddSingleton(this.ToastService.Object);
        this.Context.Services.AddSingleton<ServiceDeskSystem.Application.Services.Time.IUserTimeZone>(this.TimeZone);
    }

    public BunitContext Context { get; }

    public Mock<ILocalizationService> Localization { get; }

    public Mock<IThemeService> Theme { get; }

    public Mock<IAuthService> Auth { get; }

    public Mock<ISlaPolicyService> SlaPolicyService { get; }

    public Mock<IToastService> ToastService { get; }

    public Mock<IPreferencesService> Preferences { get; }

    public ServiceDeskSystem.Application.Services.Time.UserTimeZone TimeZone { get; } = new ();

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.Context.Dispose();
        this.disposed = true;
    }
}
