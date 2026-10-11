using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Components.UI.Primitives;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Calendar;

/// <summary>
/// Month calendar of ticket start dates and deadlines; a day agenda on narrow screens.
/// </summary>
public partial class CalendarPage : BaseComponent
{
    private const int ChipsPerDay = 3;

    private static readonly IReadOnlyList<PathSegment> Segments = [new ("calendar")];

    private DateOnly month;
    private DateOnly? selectedDay;
    private List<Ticket> tickets = [];
    private bool isLoading = true;

    [Inject]
    private ITicketService TicketService { get; set; } = null!;

    private DateOnly Today => this.UserZone.Today;

    private string MonthTitle => this.month.ToString("MMMM yyyy", this.UiCulture).ToLower(this.UiCulture);

    protected override async Task OnInitializedAsync()
    {
        this.month = FirstOfMonth(this.Today);

        var user = this.AuthService.CurrentUser;
        if (user is not null)
        {
            this.tickets = user.Role switch
            {
                UserRole.Admin => await this.TicketService.GetAllTicketsAsync(),
                UserRole.Developer => await this.TicketService.GetDeveloperTicketsAsync(user.Id),
                _ => await this.TicketService.GetUserTicketsAsync(user.Id),
            };
        }

        this.isLoading = false;
    }

    /// <inheritdoc/>
    protected override void OnTimeZoneChanged(object? sender, EventArgs e) => this.InvokeAsync(() =>
    {
        // Fires once right after the first render: "today" may be another date in the browser's zone.
        this.month = FirstOfMonth(this.Today);
        this.StateHasChanged();
    });

    private static DateOnly FirstOfMonth(DateOnly date) => new (date.Year, date.Month, 1);

    /// <summary>
    /// Monday on or before the first of the month: the first cell of the grid.
    /// </summary>
    private static DateOnly GridStart(DateOnly first) => first.AddDays(-(((int)first.DayOfWeek + 6) % 7));

    private static string Marker(CalendarEventKind kind) => kind == CalendarEventKind.Start ? "→" : "!";

    private static string KindText(CalendarEventKind kind) => kind switch
    {
        CalendarEventKind.Start => "text-muted",
        CalendarEventKind.Due => "text-fg",
        _ => "text-danger",
    };

    private static string KindClass(CalendarEventKind kind) =>
        (kind == CalendarEventKind.Overdue ? "border-danger " : "border-line ") + KindText(kind);

    private IReadOnlyDictionary<DateOnly, IReadOnlyList<CalendarEvent>> Events() =>
        CalendarEvents.ForMonth(this.tickets, this.month.Year, this.month.Month, this.ToLocal, DateTime.UtcNow);

    private IEnumerable<string> DayNames()
    {
        var names = this.UiCulture.DateTimeFormat.AbbreviatedDayNames;
        return Enumerable.Range(1, 7).Select(i => names[i % 7]);
    }

    private void ShowMonth(int delta)
    {
        this.month = this.month.AddMonths(delta);
        this.selectedDay = null;
    }

    private void ShowToday()
    {
        this.month = FirstOfMonth(this.Today);
        this.selectedDay = null;
    }

    private void ToggleDay(DateOnly day) => this.selectedDay = this.selectedDay == day ? null : day;

    private string KindLabel(CalendarEventKind kind) => this.L.Translate(kind switch
    {
        CalendarEventKind.Start => "calendar.contactDate",
        CalendarEventKind.Due => "calendar.deadline",
        _ => "calendar.overdue",
    });
}
