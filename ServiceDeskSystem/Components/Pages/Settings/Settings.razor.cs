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
    private static readonly Dictionary<string, string> AccentColors = new ()
    {
        { "blue", "Blue" },
        { "purple", "Purple" },
        { "emerald", "Emerald" },
        { "rose", "Rose" },
        { "amber", "Amber" },
        { "cyan", "Cyan" },
        { "orange", "Orange" },
        { "slate", "Slate" },
    };

    private bool isAdmin;
    private bool soundEnabled = true;
    private bool desktopNotificationsEnabled;
    private string defaultView = "TABLE";

    // Visual Effects
    private bool animationsEnabled = true;
    private string accentColor = "blue";
    private bool keyboardShortcutsEnabled = true;
    private string tableDensity = "comfortable";
    private int fontSize = 100;

    // Appearance extras
    private string dateFormat = "DD/MM/YYYY";
    private string cardStyle = "elevated";
    private string hoverStyle = "standard";
    private string bgTexture = "clean";

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

            // Font size
            var fontStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.fontSize");
            if (int.TryParse(fontStr, out var parsedFont) && parsedFont is >= 85 and <= 115)
            {
                this.fontSize = parsedFont;
            }

            // Date format
            var dateStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.dateFormat");
            this.dateFormat = !string.IsNullOrWhiteSpace(dateStr) ? dateStr : "DD/MM/YYYY";

            // Visual Settings
            var cardStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.cardStyle");
            this.cardStyle = !string.IsNullOrWhiteSpace(cardStr) ? cardStr : "elevated";

            var hoverStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.hoverStyle");
            this.hoverStyle = !string.IsNullOrWhiteSpace(hoverStr) ? hoverStr : "standard";

            var bgStr = await this.JS.InvokeAsync<string?>("localStorage.getItem", "settings.bgTexture");
            this.bgTexture = !string.IsNullOrWhiteSpace(bgStr) ? bgStr : "clean";

            await this.ApplyAccentColorAsync();
            await this.ApplyTableDensityAsync();
            await this.ApplyFontSizeAsync();
            await this.ApplyCardStyleAsync();
            await this.ApplyHoverStyleAsync();
            await this.ApplyBgTextureAsync();
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
        "cyan" => "background: linear-gradient(135deg, #06b6d4, #0ea5e9); color: #06b6d4;",
        "orange" => "background: linear-gradient(135deg, #f97316, #ef4444); color: #f97316;",
        "slate" => "background: linear-gradient(135deg, #64748b, #475569); color: #64748b;",
        _ => "background: linear-gradient(135deg, #3b82f6, #6366f1); color: #3b82f6;",
    };

    /// <summary>Returns the solid hex color for live preview dot/button.</summary>
    private static string GetAccentPreviewColor(string color) => color switch
    {
        "blue" => "#3b82f6",
        "purple" => "#8b5cf6",
        "emerald" => "#10b981",
        "rose" => "#f43f5e",
        "amber" => "#f59e0b",
        "cyan" => "#06b6d4",
        "orange" => "#f97316",
        "slate" => "#64748b",
        _ => "#3b82f6",
    };

    /// <summary>Maps 85-115 font-size value to a CSS px string for live preview.</summary>
    private static string GetFontSizePx(int pct) => pct switch
    {
        85 => "0.65rem",
        90 => "0.7rem",
        95 => "0.7rem",
        100 => "0.72rem",
        105 => "0.8rem",
        110 => "0.85rem",
        115 => "0.9rem",
        _ => "0.72rem",
    };

    /// <summary>Returns today's date formatted according to the current dateFormat setting.</summary>
    private string FormatPreviewDate()
    {
        var now = DateTime.Now;
        return this.dateFormat switch
        {
            "MM/DD/YYYY" => now.ToString("MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture),
            "YYYY-MM-DD" => now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            _ => now.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
        };
    }

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

    private void OnFontSizeInput(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var val))
        {
            this.fontSize = val;
        }
    }

    private async Task OnFontSizeChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var val))
        {
            this.fontSize = val;
            await this.SaveSetting("settings.fontSize", val.ToString(System.Globalization.CultureInfo.InvariantCulture));
            await this.ApplyFontSizeAsync();
        }
    }

    private async Task SetDateFormat(string format)
    {
        this.dateFormat = format;
        await this.SaveSetting("settings.dateFormat", format);
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

    private async Task ApplyFontSizeAsync()
    {
        try
        {
            await this.JS.InvokeVoidAsync("eval", $"document.documentElement.style.fontSize = '{this.fontSize}%'");
        }
        catch
        {
            // Ignore JS interop errors during prerendering.
        }
    }

    private async Task SetCardStyle(string style)
    {
        this.cardStyle = style;
        await this.SaveSetting("settings.cardStyle", style);
        await this.ApplyCardStyleAsync();
    }

    private async Task ApplyCardStyleAsync()
    {
        try
        {
            await this.JS.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-card-style','{this.cardStyle}')");
        }
        catch
        {
            // Ignore JS interop errors
        }
    }

    private async Task SetHoverStyle(string style)
    {
        this.hoverStyle = style;
        await this.SaveSetting("settings.hoverStyle", style);
        await this.ApplyHoverStyleAsync();
    }

    private async Task ApplyHoverStyleAsync()
    {
        try
        {
            await this.JS.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-hover-style','{this.hoverStyle}')");
        }
        catch
        {
            // Ignore JS interop errors
        }
    }

    private async Task SetBgTexture(string texture)
    {
        this.bgTexture = texture;
        await this.SaveSetting("settings.bgTexture", texture);
        await this.ApplyBgTextureAsync();
    }

    private async Task ApplyBgTextureAsync()
    {
        try
        {
            await this.JS.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-bg-texture','{this.bgTexture}')");
        }
        catch
        {
            // Ignore JS interop errors
        }
    }

    private async Task SetSidebarMode(string mode)
    {
        await this.Theme.SetSidebarCollapsedAsync(mode == "compact");
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
