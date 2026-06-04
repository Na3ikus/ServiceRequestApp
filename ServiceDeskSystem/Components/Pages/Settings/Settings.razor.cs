using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Components.Pages.Settings;

/// <summary>
/// Application settings page with appearance, notifications, and system info.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Blazor page component must match .razor file name")]
public partial class Settings : BaseComponent
{
    private bool isAdmin;
    private bool soundEnabled = true;
    private bool desktopNotificationsEnabled;
    private string defaultView = "table";

    // Visual Effects
    private bool animationsEnabled = true;
    private string accentColor = "blue";
    private bool keyboardShortcutsEnabled = true;
    private string tableDensity = "comfortable";

    // System health
    private bool dbAvailable;
    private bool smtpAvailable;
    private bool isCheckingDb;
    private bool isCheckingSmtp;
    private string smtpMessage = "SMTP";

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    [Inject]
    private IHttpClientFactory HttpClientFactory { get; set; } = null!;

    [Inject]
    private IEmailSender EmailSender { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        if (this.AuthService.CurrentUser is null)
        {
            this.Navigation.NavigateTo("/login", replace: true);
            return;
        }

        this.isAdmin = this.AuthService.CurrentUser.Role == UserRole.Admin;

        try
        {
            var soundStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.soundEnabled");
            this.soundEnabled = soundStr != "false";
            var viewStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.defaultView");
            this.defaultView = !string.IsNullOrWhiteSpace(viewStr) ? viewStr.ToUpperInvariant() : "TABLE";
            var desktopStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.desktopNotifications");
            this.desktopNotificationsEnabled = desktopStr == "true";

            // Visual Effects
            var animStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.animations");
            this.animationsEnabled = animStr != "false";
            var accentStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.accentColor");
            this.accentColor = !string.IsNullOrWhiteSpace(accentStr) ? accentStr : "blue";
            var kbStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.keyboardShortcuts");
            this.keyboardShortcutsEnabled = kbStr != "false";
            var densityStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.tableDensity");
            this.tableDensity = !string.IsNullOrWhiteSpace(densityStr) ? densityStr : "comfortable";

            await this.ApplyAccentColorAsync();
            await this.ApplyTableDensityAsync();
        }
        catch
        {
            // localStorage unavailable during prerender
        }

        if (this.isAdmin)
        {
            await this.CheckSystemStatusAsync();
        }
    }

    private static string GetAccentSwatchStyle(string color) => color switch
    {
        "blue" => "background: linear-gradient(135deg, #3b82f6, #6366f1); color: #3b82f6;",
        "purple" => "background: linear-gradient(135deg, #8b5cf6, #a855f7); color: #8b5cf6;",
        "emerald" => "background: linear-gradient(135deg, #10b981, #14b8a6); color: #10b981;",
        "rose" => "background: linear-gradient(135deg, #f43f5e, #e11d48); color: #f43f5e;",
        "amber" => "background: linear-gradient(135deg, #f59e0b, #f97316); color: #f59e0b;",
        _ => "background: linear-gradient(135deg, #3b82f6, #6366f1); color: #3b82f6;",
    };

    private void SetTheme(bool dark)
    {
        this.Theme.SetTheme(dark ? "dark" : "light");
    }

    private async Task ToggleSoundNotifications()
    {
        this.soundEnabled = !this.soundEnabled;
        await this.SaveSetting("settings.soundEnabled", this.soundEnabled ? "true" : "false");
    }

    private async Task ToggleDesktopNotifications()
    {
        this.desktopNotificationsEnabled = !this.desktopNotificationsEnabled;
        await this.SaveSetting("settings.desktopNotifications", this.desktopNotificationsEnabled ? "true" : "false");
    }

    private async Task SetDefaultView(string view)
    {
        this.defaultView = view;
        await this.SaveSetting("settings.defaultView", view);
    }

    private async Task ToggleAnimations()
    {
        this.animationsEnabled = !this.animationsEnabled;
        await this.SaveSetting("settings.animations", this.animationsEnabled ? "true" : "false");
    }

    private async Task SetAccentColor(string color)
    {
        this.accentColor = color;
        await this.SaveSetting("settings.accentColor", color);
        await this.ApplyAccentColorAsync();
    }

    private async Task ToggleKeyboardShortcuts()
    {
        this.keyboardShortcutsEnabled = !this.keyboardShortcutsEnabled;
        await this.SaveSetting("settings.keyboardShortcuts", this.keyboardShortcutsEnabled ? "true" : "false");
    }

    private async Task ApplyAccentColorAsync()
    {
        try
        {
            if (this.accentColor == "blue")
            {
                await this.JS.InvokeVoidAsync("eval", "document.documentElement.removeAttribute('data-accent')");
            }
            else
            {
                await this.JS.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-accent','{this.accentColor}')");
            }
        }
        catch
        {
            // Ignore JS interop errors during prerendering.
        }
    }

    private async Task SetTableDensity(string density)
    {
        this.tableDensity = density;
        await this.SaveSetting("settings.tableDensity", density);
        await this.ApplyTableDensityAsync();
    }

    private async Task ApplyTableDensityAsync()
    {
        try
        {
            await this.JS.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-density','{this.tableDensity}')");
        }
        catch
        {
            // Ignore JS interop errors during prerendering.
        }
    }

    private async Task SaveSetting(string key, string value)
    {
        try
        {
            await this.JS.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch
        {
            // Ignore JS interop errors during prerendering.
        }
    }

    private async Task CheckSystemStatusAsync()
    {
        this.isCheckingDb = true;
        this.isCheckingSmtp = true;
        this.StateHasChanged();

        // Check DB
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
            this.StateHasChanged();
        }

        // Check SMTP
        try
        {
            var result = await this.EmailSender.CheckConnectionAsync();
            this.smtpAvailable = result.IsSuccess;
            this.smtpMessage = result.Message;
        }
        catch (Exception ex)
        {
            this.smtpAvailable = false;
            this.smtpMessage = ex.Message;
        }
        finally
        {
            this.isCheckingSmtp = false;
            this.StateHasChanged();
        }
    }
}
