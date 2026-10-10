using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Domain.Entities;

/// <summary>
/// One status change of a ticket. Kept separately from the audit log so it survives
/// an audit-log cleanup (the profile activity map and future "closed at" figures use it).
/// </summary>
public class TicketStatusHistory
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public TicketStatus OldStatus { get; set; }

    public TicketStatus NewStatus { get; set; }

    public int? ChangedByUserId { get; set; }

    public User? ChangedByUser { get; set; }

    public DateTime ChangedAt { get; set; }
}
