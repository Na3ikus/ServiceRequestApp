using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Localization;
using ServiceDeskSystem.Application.Services.Localization.Interfaces;
using ServiceDeskSystem.Application.Services.Theme;
using ServiceDeskSystem.Application.Services.Theme.Interfaces;
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

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

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

    protected string GetStatusBadgeClass(TicketStatus status) => status switch
    {
        TicketStatus.New => "bg-purple-200 text-purple-900 dark:bg-purple-900 dark:text-purple-200 font-semibold",
        TicketStatus.Open => "bg-blue-200 text-blue-900 dark:bg-blue-900 dark:text-blue-200 font-semibold",
        TicketStatus.InProgress => "bg-yellow-200 text-yellow-900 dark:bg-yellow-900 dark:text-yellow-200 font-semibold",
        TicketStatus.Resolved => "bg-green-200 text-green-900 dark:bg-green-900 dark:text-green-200 font-semibold",
        TicketStatus.Closed => "bg-gray-200 text-gray-900 dark:bg-gray-700 dark:text-gray-200 font-semibold",
        TicketStatus.Testing => "bg-cyan-200 text-cyan-900 dark:bg-cyan-900 dark:text-cyan-200 font-semibold",
        TicketStatus.CodeReview => "bg-indigo-200 text-indigo-900 dark:bg-indigo-900 dark:text-indigo-200 font-semibold",
        TicketStatus.Done => "bg-emerald-200 text-emerald-900 dark:bg-emerald-900 dark:text-emerald-200 font-semibold",
        _ => "bg-gray-200 text-gray-900 dark:bg-gray-700 dark:text-gray-200 font-semibold",
    };

    protected string GetPriorityBadgeClass(TicketPriority priority) => priority switch
    {
        TicketPriority.Critical => "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200",
        TicketPriority.High => "bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200",
        TicketPriority.Medium => "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200",
        TicketPriority.Low => "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
        _ => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200",
    };

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
    private void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);
}
