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

    protected bool IsAdminOrDeveloper => this.AuthService.CurrentUser?.Role is UserRole.Admin or UserRole.Developer;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void ViewTicket(int id) => this.Navigation.NavigateTo($"/ticket/{id}");

    protected override void OnInitialized()
    {
        this.L.LanguageChanged += this.OnStateChanged;
        this.Theme.ThemeChanged += this.OnStateChanged;
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
}
