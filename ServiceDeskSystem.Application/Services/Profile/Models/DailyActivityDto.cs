namespace ServiceDeskSystem.Application.Services.Profile.Models;

/// <summary>
/// What one user did on one calendar day. Every action weighs 1 in <see cref="Total"/>.
/// </summary>
public sealed record DailyActivityDto(
    DateOnly Date,
    int TicketsCreated,
    int Comments,
    int StatusChanges,
    int WorkLogs,
    int WorkMinutes)
{
    public int Total => this.TicketsCreated + this.Comments + this.StatusChanges + this.WorkLogs;
}
