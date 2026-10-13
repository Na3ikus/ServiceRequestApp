namespace ServiceDeskSystem.Application.Services.Tickets;

/// <summary>
/// Outcome of an edit to an existing ticket.
/// </summary>
public enum TicketUpdateResult
{
    /// <summary>The edit was saved.</summary>
    Updated,

    /// <summary>There is no such ticket.</summary>
    NotFound,

    /// <summary>Someone changed the ticket after the caller loaded it; nothing was saved.</summary>
    Conflict,
}
