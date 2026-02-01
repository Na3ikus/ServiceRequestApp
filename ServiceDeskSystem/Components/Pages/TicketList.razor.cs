using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Data.Entities;
using ServiceDeskSystem.Services.Localization;
using ServiceDeskSystem.Services.Theme;
using ServiceDeskSystem.Services.Tickets;

namespace ServiceDeskSystem.Components.Pages;

/// <summary>
/// Ticket list page component.
/// </summary>
public partial class TicketList : IDisposable
{
    private bool disposed;

    [Inject]
    private ITicketService TicketService { get; set; } = null!;

    [Inject]
    private ILocalizationService L { get; set; } = null!;

    [Inject]
    private IThemeService Theme { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private List<Ticket>? tickets { get; set; }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override async Task OnInitializedAsync()
    {
        this.L.LanguageChanged += this.OnStateChanged;
        this.Theme.ThemeChanged += this.OnStateChanged;
        this.tickets = await this.TicketService.GetAllTicketsAsync();
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

    private static string GetStatusBadgeClass(string status) => status switch
    {
        "Open" => "bg-blue-100 text-blue-800 dark:bg-blue-900/50 dark:text-blue-300",
        "In Progress" => "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/50 dark:text-yellow-300",
        "Resolved" => "bg-green-100 text-green-800 dark:bg-green-900/50 dark:text-green-300",
        "Closed" => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
        _ => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
    };

    private static string GetPriorityBadgeClass(string priority) => priority switch
    {
        "Critical" => "bg-red-100 text-red-800 dark:bg-red-900/50 dark:text-red-300",
        "High" => "bg-orange-100 text-orange-800 dark:bg-orange-900/50 dark:text-orange-300",
        "Medium" => "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/50 dark:text-yellow-300",
        "Low" => "bg-green-100 text-green-800 dark:bg-green-900/50 dark:text-green-300",
        _ => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
    };

    private void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);

    private void ViewTicket(int id) => this.Navigation.NavigateTo($"/ticket/{id}");

    private string GetStatusText(string status) => status switch
    {
        "Open" => this.L.Translate("status.open"),
        "In Progress" => this.L.Translate("status.inProgress"),
        "Resolved" => this.L.Translate("status.resolved"),
        "Closed" => this.L.Translate("status.closed"),
        _ => status,
    };

    private string GetPriorityText(string priority) => priority switch
    {
        "Low" => this.L.Translate("priority.low"),
        "Medium" => this.L.Translate("priority.medium"),
        "High" => this.L.Translate("priority.high"),
        "Critical" => this.L.Translate("priority.critical"),
        _ => priority,
    };
}