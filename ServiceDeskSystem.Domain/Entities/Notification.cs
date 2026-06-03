namespace ServiceDeskSystem.Domain.Entities;

public class Notification
{
    public int Id { get; set; }

    public int RecipientUserId { get; set; }

    public User RecipientUser { get; set; } = null!;

    public int? ActorUserId { get; set; }

    public User? ActorUser { get; set; }

    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public string Type { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}

