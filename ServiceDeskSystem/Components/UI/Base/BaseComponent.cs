using System.Globalization;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.UI.Base;

/// <summary>
/// Base component class with common functionality for localization, theming, and disposal.
/// </summary>
public abstract class BaseComponent : ComponentBase, IDisposable
{
    protected bool disposed;

    [Inject]
    protected ILocalizationService L { get; set; } = null!;

    [Inject]
    protected IThemeService Theme { get; set; } = null!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = null!;

    [Inject]
    protected ServiceDeskSystem.Application.Services.Auth.IAuthService AuthService { get; set; } = null!;

    /// <summary>
    /// Gets or sets the browser's time zone; show stored UTC times through <see cref="ToLocal"/>.
    /// </summary>
    [Inject]
    protected ServiceDeskSystem.Application.Services.Time.IUserTimeZone UserZone { get; set; } = null!;

    protected bool IsAdminOrDeveloper => this.AuthService.CurrentUser?.Role is UserRole.Admin or UserRole.Developer;

    /// <summary>
    /// Gets the culture of the selected UI language (dates, month and day names).
    /// </summary>
    protected CultureInfo UiCulture
    {
        get
        {
            var lang = this.L.SupportedLanguages.FirstOrDefault(l => string.Equals(l.Code, this.L.CurrentLanguage, StringComparison.OrdinalIgnoreCase));
            try
            {
                return CultureInfo.GetCultureInfo(lang?.CultureName ?? this.L.CurrentLanguage);
            }
            catch (CultureNotFoundException)
            {
                return CultureInfo.InvariantCulture;
            }
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void ViewTicket(int id) => this.Navigation.NavigateTo($"/ticket/{id}");

    /// <summary>
    /// Converts a stored UTC timestamp to the user's time zone.
    /// </summary>
    protected DateTime ToLocal(DateTime utc) => this.UserZone.ToLocal(utc);

    /// <summary>
    /// Section heading text: translated and lowercased in the UI culture (<c># overview</c>).
    /// </summary>
    protected string Heading(string key) => this.L.Translate(key).ToLower(this.UiCulture);

    protected override void OnInitialized()
    {
        this.L.LanguageChanged += this.OnStateChanged;
        this.Theme.ThemeChanged += this.OnStateChanged;
        this.UserZone.Changed += this.OnTimeZoneChanged;
        base.OnInitialized();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this.disposed)
        {
            return;
        }

        if (disposing)
        {
            this.L.LanguageChanged -= this.OnStateChanged;
            this.Theme.ThemeChanged -= this.OnStateChanged;
            this.UserZone.Changed -= this.OnTimeZoneChanged;
        }

        this.disposed = true;
    }

    protected string GetStatusText(TicketStatus status) => status switch
    {
        TicketStatus.New => this.L.Translate("status.new"),
        TicketStatus.Open => this.L.Translate("status.open"),
        TicketStatus.InProgress => this.L.Translate("status.inProgress"),
        TicketStatus.Testing => this.L.Translate("status.testing"),
        TicketStatus.CodeReview => this.L.Translate("status.codeReview"),
        TicketStatus.Resolved => this.L.Translate("status.resolved"),
        TicketStatus.Done => this.L.Translate("status.done"),
        TicketStatus.Closed => this.L.Translate("status.closed"),
        _ => status.ToString(),
    };

    protected string GetPriorityText(TicketPriority priority) => priority switch
    {
        TicketPriority.Low => this.L.Translate("priority.low"),
        TicketPriority.Medium => this.L.Translate("priority.medium"),
        TicketPriority.High => this.L.Translate("priority.high"),
        TicketPriority.Critical => this.L.Translate("priority.critical"),
        _ => priority.ToString(),
    };

    protected string GetTicketTypeText(TicketType type) => type switch
    {
        TicketType.Bug => this.L.Translate("ticketType.bug"),
        TicketType.Support => this.L.Translate("ticketType.support"),
        TicketType.Consultation => this.L.Translate("ticketType.consultation"),
        TicketType.Project => this.L.Translate("ticketType.project"),
        _ => type.ToString(),
    };

    protected virtual void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);

    /// <summary>
    /// Runs when the browser reports its time zone (after the first render); re-renders by default.
    /// </summary>
    protected virtual void OnTimeZoneChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);
}
