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
    private readonly CancellationTokenSource animationCts = new();

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
            this.animationCts.Cancel();
            this.animationCts.Dispose();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        this.L.LanguageChanged += this.OnStateChanged;
        this.Theme.ThemeChanged += this.OnStateChanged;

        Task<int> totalTicketsTask = this.TicketStatisticsService.GetTotalTicketsCountAsync();
        Task<int> openTicketsTask = this.TicketStatisticsService.GetOpenTicketsCountAsync();
        Task<int> criticalTicketsTask = this.TicketStatisticsService.GetCriticalTicketsCountAsync();

        Task<int> myTicketsTask = Task.FromResult(0);
        if (this.AuthService.CurrentUser is not null)
        {
            myTicketsTask = this.TicketStatisticsService.GetUserTicketsCountAsync(this.AuthService.CurrentUser.Id);
        }

        await Task.WhenAll(totalTicketsTask, openTicketsTask, criticalTicketsTask, myTicketsTask);

        this.totalTickets = await totalTicketsTask;
        this.openTickets = await openTicketsTask;
        this.criticalTickets = await criticalTicketsTask;
        this.myTickets = await myTicketsTask;

        _ = this.AnimateNumbersAsync(this.animationCts.Token);
    }

    private async Task AnimateNumbersAsync(CancellationToken cancellationToken)
    {
        const int Steps = 25;
        const int DelayMs = 15;

        for (int i = 1; i <= Steps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            float progress = (float)i / Steps;
            float eased = progress * (2 - progress);

            this.displayTotalTickets = (int)(this.totalTickets * eased);
            this.displayOpenTickets = (int)(this.openTickets * eased);
            this.displayCriticalTickets = (int)(this.criticalTickets * eased);
            this.displayMyTickets = (int)(this.myTickets * eased);

            await this.InvokeAsync(this.StateHasChanged);
            await Task.Delay(DelayMs, cancellationToken);
        }

        this.displayTotalTickets = this.totalTickets;
        this.displayOpenTickets = this.openTickets;
        this.displayCriticalTickets = this.criticalTickets;
        this.displayMyTickets = this.myTickets;
        await this.InvokeAsync(this.StateHasChanged);
    }

    private void OnStateChanged(object? sender, EventArgs e) => _ = this.InvokeAsync(this.StateHasChanged);
}
