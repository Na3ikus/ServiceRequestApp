using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Localization.Interfaces;
using ServiceDeskSystem.Application.Services.Theme.Interfaces;
using ServiceDeskSystem.Application.Services.Tickets.Interfaces;

namespace ServiceDeskSystem.Components.UI.Elements;

public partial class DashboardStats : ComponentBase, IDisposable
{
    protected int displayTotalTickets;
    protected int displayOpenTickets;
    protected int displayCriticalTickets;
    protected int displayMyTickets;

    private int totalTickets;
    private int openTickets;
    private int criticalTickets;
    private int myTickets;

    [Inject]
    protected ITicketStatisticsService TicketStatisticsService { get; set; } = null!;

    [Inject]
    protected IAuthService AuthService { get; set; } = null!;

    [Inject]
    protected ILocalizationService L { get; set; } = null!;

    [Inject]
    protected IThemeService Theme { get; set; } = null!;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.L.LanguageChanged -= this.OnStateChanged;
            this.Theme.ThemeChanged -= this.OnStateChanged;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        this.L.LanguageChanged += this.OnStateChanged;
        this.Theme.ThemeChanged += this.OnStateChanged;

        this.totalTickets = await this.TicketStatisticsService.GetTotalTicketsCountAsync();
        this.openTickets = await this.TicketStatisticsService.GetOpenTicketsCountAsync();
        this.criticalTickets = await this.TicketStatisticsService.GetCriticalTicketsCountAsync();

        if (this.AuthService.CurrentUser is not null)
        {
            this.myTickets = await this.TicketStatisticsService.GetUserTicketsCountAsync(this.AuthService.CurrentUser.Id);
        }

        _ = this.AnimateNumbersAsync();
    }

    private async Task AnimateNumbersAsync()
    {
        const int Steps = 25;
        const int DelayMs = 15;

        for (int i = 1; i <= Steps; i++)
        {
            float progress = (float)i / Steps;
            float eased = progress * (2 - progress);

            this.displayTotalTickets = (int)(this.totalTickets * eased);
            this.displayOpenTickets = (int)(this.openTickets * eased);
            this.displayCriticalTickets = (int)(this.criticalTickets * eased);
            this.displayMyTickets = (int)(this.myTickets * eased);

            await this.InvokeAsync(this.StateHasChanged);
            await Task.Delay(DelayMs);
        }

        this.displayTotalTickets = this.totalTickets;
        this.displayOpenTickets = this.openTickets;
        this.displayCriticalTickets = this.criticalTickets;
        this.displayMyTickets = this.myTickets;
        await this.InvokeAsync(this.StateHasChanged);
    }

    private void OnStateChanged(object? sender, EventArgs e) => _ = this.InvokeAsync(this.StateHasChanged);
}
