using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Localization;
using ServiceDeskSystem.Application.Services.Theme;

namespace ServiceDeskSystem.Components.Common;

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

    private void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);

    protected string GetStatusBadgeClass(string status) => status switch
    {
        "New" => "bg-purple-200 text-purple-900 dark:bg-purple-900 dark:text-purple-200 font-semibold",
        "Open" => "bg-blue-200 text-blue-900 dark:bg-blue-900 dark:text-blue-200 font-semibold",
        "In Progress" => "bg-yellow-200 text-yellow-900 dark:bg-yellow-900 dark:text-yellow-200 font-semibold",
        "Resolved" => "bg-green-200 text-green-900 dark:bg-green-900 dark:text-green-200 font-semibold",
        "Closed" => "bg-gray-200 text-gray-900 dark:bg-gray-700 dark:text-gray-200 font-semibold",
        "Testing" => "bg-cyan-200 text-cyan-900 dark:bg-cyan-900 dark:text-cyan-200 font-semibold",
        "Code Review" => "bg-indigo-200 text-indigo-900 dark:bg-indigo-900 dark:text-indigo-200 font-semibold",
        "Done" => "bg-emerald-200 text-emerald-900 dark:bg-emerald-900 dark:text-emerald-200 font-semibold",
        _ => "bg-gray-200 text-gray-900 dark:bg-gray-700 dark:text-gray-200 font-semibold",
    };

    protected string GetPriorityBadgeClass(string priority) => priority switch
    {
        "Critical" => "bg-red-100 text-red-800 dark:bg-red-900/50 dark:text-red-300",
        "High" => "bg-orange-100 text-orange-800 dark:bg-orange-900/50 dark:text-orange-300",
        "Medium" => "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/50 dark:text-yellow-300",
        "Low" => "bg-green-100 text-green-800 dark:bg-green-900/50 dark:text-green-300",
        _ => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
    };

    protected string GetStatusText(string status) => status switch
    {
        "New" => this.L.Translate("status.new"),
        "Open" => this.L.Translate("status.open"),
        "In Progress" => this.L.Translate("status.inProgress"),
        "Testing" => this.L.Translate("status.testing"),
        "Code Review" => this.L.Translate("status.codeReview"),
        "Resolved" => this.L.Translate("status.resolved"),
        "Done" => this.L.Translate("status.done"),
        "Closed" => this.L.Translate("status.closed"),
        _ => status,
    };

    protected string GetPriorityText(string priority) => priority switch
    {
        "Low" => this.L.Translate("priority.low"),
        "Medium" => this.L.Translate("priority.medium"),
        "High" => this.L.Translate("priority.high"),
        "Critical" => this.L.Translate("priority.critical"),
        _ => priority,
    };
}
