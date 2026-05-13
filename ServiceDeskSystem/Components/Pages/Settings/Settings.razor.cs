using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Components.Pages.Settings;

/// <summary>
/// Application settings page with appearance, notifications, and system info.
/// </summary>
public partial class Settings : BaseComponent
{
    private bool isAdmin;
    private bool soundEnabled = true;
    private bool desktopNotificationsEnabled;
    private string defaultView = "table";

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
    private NavigationManager Navigation { get; set; } = null!;

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

        this.isAdmin = this.AuthService.CurrentUser.Role == "Admin";

        try
        {
            var soundStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.soundEnabled");
            this.soundEnabled = soundStr != "false";
            var viewStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.defaultView");
            this.defaultView = !string.IsNullOrWhiteSpace(viewStr) ? viewStr.ToUpperInvariant() : "TABLE";
            var desktopStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.desktopNotifications");
            this.desktopNotificationsEnabled = desktopStr == "true";
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

    private void SetTheme(bool dark)
    {
        this.Theme.SetTheme(dark ? "dark" : "light");
    }

    private async Task ToggleSoundNotifications()
    {
        this.soundEnabled = !this.soundEnabled;
        await this.SaveSetting("settings.soundEnabled", this.soundEnabled.ToString().ToLowerInvariant());
    }

    private async Task ToggleDesktopNotifications()
    {
        this.desktopNotificationsEnabled = !this.desktopNotificationsEnabled;
        await this.SaveSetting("settings.desktopNotifications", this.desktopNotificationsEnabled.ToString().ToLowerInvariant());
    }

    private async Task SetDefaultView(string view)
    {
        this.defaultView = view;
        await this.SaveSetting("settings.defaultView", view);
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
            var response = await client.GetAsync("health/db", cts.Token);
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
