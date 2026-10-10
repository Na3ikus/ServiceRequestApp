using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Preferences;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Components.Pages.Settings;

/// <summary>
/// User preferences page. Every control persists through <see cref="IPreferencesService"/>
/// (or <c>IThemeService</c> / <c>ILocalizationService</c> for theme, sidebar and language).
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Blazor page component must match .razor file name")]
public partial class Settings : BaseComponent
{
    // Option texts are value tokens shown in mono, like the section headings; not translated.
    private static readonly IReadOnlyList<(string Value, string Text)> ThemeOptions =
        [("light", "light"), ("dark", "dark"), ("system", "system")];

    private static readonly IReadOnlyList<(string Value, string Text)> ScaleOptions =
        [("90", "90%"), ("100", "100%"), ("110", "110%"), ("120", "120%")];

    private static readonly IReadOnlyList<(string Value, string Text)> DensityOptions =
        [("comfortable", "comfortable"), ("compact", "compact")];

    private static readonly IReadOnlyList<(string Value, string Text)> WidthOptions =
        [("narrow", "narrow"), ("standard", "standard"), ("wide", "wide")];

    private static readonly IReadOnlyList<(string Value, string Text)> SidebarOptions =
        [("full", "full"), ("compact", "compact")];

    private static readonly IReadOnlyList<(string Value, string Text)> DateFormatOptions =
        [("DD/MM/YYYY", "dd/mm/yyyy"), ("MM/DD/YYYY", "mm/dd/yyyy"), ("YYYY-MM-DD", "yyyy-mm-dd")];

    private static readonly IReadOnlyList<(string Value, string Text)> ViewOptions =
        [("TABLE", "table"), ("KANBAN", "kanban")];

    private static readonly IReadOnlyList<(string Value, string Text)> ToastOptions =
        [("top-right", "top-right"), ("top-center", "top-center"), ("bottom-right", "bottom-right"), ("bottom-center", "bottom-center")];

    private static readonly string AppVersion = "v" + (typeof(Settings).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0");

    private UserPreferences prefs = UserPreferences.Defaults;
    private bool isAdmin;
    private bool desktopDenied;
    private bool showResetConfirm;

    private bool dbAvailable;
    private bool smtpAvailable;
    private bool isCheckingDb;
    private bool isCheckingSmtp;
    private string smtpMessage = string.Empty;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    [Inject]
    private IHttpClientFactory HttpClientFactory { get; set; } = null!;

    [Inject]
    private IEmailSender EmailSender { get; set; } = null!;

    [Inject]
    private IPreferencesService Prefs { get; set; } = null!;

    private string ThemeValue => this.Theme.IsSystemTheme ? "system" : this.Theme.CurrentTheme;

    private IReadOnlyList<(string Value, string Text)> LanguageOptions =>
        this.L.SupportedLanguages.Select(l => (l.Code, l.Code)).ToList();

    protected override async Task OnInitializedAsync()
    {
        if (this.AuthService.CurrentUser is null)
        {
            this.Navigation.NavigateTo("/login", replace: true);
            return;
        }

        this.isAdmin = this.AuthService.CurrentUser.Role == UserRole.Admin;
        this.prefs = await this.Prefs.LoadAsync();

        if (this.isAdmin)
        {
            await this.CheckSystemStatusAsync();
        }
    }

    private static string Bool(bool value) => value ? "true" : "false";

    private async Task SetAsync(string name, string value)
    {
        await this.Prefs.SetAsync(name, value);
        this.prefs = this.prefs.With(name, value);
    }

    private void SetTheme(string theme) => this.Theme.SetTheme(theme);

    private Task SetSidebarAsync(string value) => this.Theme.SetSidebarCollapsedAsync(value == "compact");

    private async Task SetLanguageAsync(string code)
    {
        this.L.SetLanguage(code);
        try
        {
            await this.JS.InvokeVoidAsync("localStorage.setItem", "settings.language", code);
        }
        catch (JSException)
        {
            // Language still applies for this session.
        }
    }

    private async Task SetDesktopNotificationsAsync(bool enabled)
    {
        if (!enabled)
        {
            this.desktopDenied = false;
            await this.SetAsync(PreferenceSchema.DesktopNotifications, "false");
            return;
        }

        string? permission;
        try
        {
            permission = await this.JS.InvokeAsync<string?>("notificationManager.requestDesktopPermission");
        }
        catch (JSException)
        {
            permission = null;
        }

        if (permission == "granted")
        {
            this.desktopDenied = false;
            await this.SetAsync(PreferenceSchema.DesktopNotifications, "true");
        }
        else
        {
            this.desktopDenied = true;
        }
    }

    private async Task ShowHotkeysHelpAsync()
    {
        try
        {
            await this.JS.InvokeVoidAsync("sidebarManager.showHotkeysHelp");
        }
        catch (JSException)
        {
            // Hotkeys modal not available on this page.
        }
    }

    private async Task ConfirmResetAsync()
    {
        await this.Prefs.ResetAsync();
        this.Theme.SetTheme("system");
        this.prefs = UserPreferences.Defaults;
        this.desktopDenied = false;
        this.showResetConfirm = false;
    }

    private async Task CheckSystemStatusAsync()
    {
        this.isCheckingDb = true;
        this.isCheckingSmtp = true;
        this.StateHasChanged();

        try
        {
            using var client = this.HttpClientFactory.CreateClient();
            client.BaseAddress = new Uri(this.Navigation.BaseUri);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await client.GetAsync(new Uri("health/db", UriKind.Relative), cts.Token);
            this.dbAvailable = response.IsSuccessStatusCode;
        }
        catch
        {
            this.dbAvailable = false;
        }
        finally
        {
            this.isCheckingDb = false;
        }

        try
        {
            var (isSuccess, message) = await this.EmailSender.CheckConnectionAsync();
            this.smtpAvailable = isSuccess;
            this.smtpMessage = message;
        }
        catch (Exception ex)
        {
            this.smtpAvailable = false;
            this.smtpMessage = ex.Message;
        }
        finally
        {
            this.isCheckingSmtp = false;
        }
    }
}
