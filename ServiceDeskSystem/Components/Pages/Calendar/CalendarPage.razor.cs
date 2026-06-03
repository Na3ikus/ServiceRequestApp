using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Calendar;

public partial class CalendarPage : BaseComponent
{
    private DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
    private List<Ticket> tickets = [];
    private bool isLoading = true;

    [Inject]
    private ITicketService TicketService { get; set; } = null!;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await this.LoadTicketsAsync();
    }

    private async Task LoadTicketsAsync()
    {
        this.isLoading = true;
        this.StateHasChanged();

        if (this.AuthService.CurrentUser is null)
        {
            this.tickets = [];
            this.isLoading = false;
            return;
        }

        if (this.AuthService.CurrentUser.Role == UserRole.Admin)
        {
            this.tickets = await this.TicketService.GetAllTicketsAsync();
        }
        else if (this.AuthService.CurrentUser.Role == UserRole.Developer)
        {
            this.tickets = await this.TicketService.GetDeveloperTicketsAsync(this.AuthService.CurrentUser.Id);
        }
        else
        {
            this.tickets = await this.TicketService.GetUserTicketsAsync(this.AuthService.CurrentUser.Id);
        }

        // We want to show tickets on the calendar. To avoid cluttering, we might only want to show:
        // 1. Tickets that are active (Open/InProgress/Testing/Review) OR
        // 2. Tickets that have a specific DueDate.
        // Let's filter to only show tickets relevant to the calendar (e.g., exclude old closed tickets without due dates)
        // But for now, let's keep all tickets and let the month filtering handle it.
        this.isLoading = false;
        this.StateHasChanged();
    }

    private void PreviousMonth()
    {
        this.currentMonth = this.currentMonth.AddMonths(-1);
    }

    private void NextMonth()
    {
        this.currentMonth = this.currentMonth.AddMonths(1);
    }

    private IEnumerable<Ticket> GetTicketsForDay(DateTime day)
    {
        return this.tickets.Where(t =>
        {
            var start = t.StartDate ?? t.CreatedAt;
            var end = t.DueDate;

            bool isStartDay = start.Date == day.Date;
            bool isDueDay = end.HasValue && end.Value.Date == day.Date;
            bool isBetween = end.HasValue && day.Date >= start.Date && day.Date <= end.Value.Date;

            return isStartDay || isDueDay || isBetween;
        }).OrderBy(t => t.DueDate ?? DateTime.MaxValue).Take(4); // Show max 4 tickets per day
    }
}
