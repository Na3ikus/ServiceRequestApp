namespace ServiceDeskSystem.Domain.Entities;

using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Common;

public class Ticket : AggregateRoot
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string StepsToReproduce { get; set; } = string.Empty;

    public TicketType Type { get; set; }

    public TicketPriority Priority { get; set; }

    public TicketStatus Status { get; set; }

    public string AffectedVersion { get; set; } = string.Empty;

    public string Environment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public int? ProductId { get; set; }

    public Product? Product { get; set; }

    public int AuthorId { get; set; }

    public User Author { get; set; } = null!;

    public int? DeveloperId { get; set; }

    public User? Developer { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public static Ticket Create(string title, string description, TicketType type, TicketPriority priority, int authorId, int? productId = null)
    {
        var ticket = new Ticket
        {
            Title = title,
            Description = description,
            Type = type,
            Priority = priority,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow,
            AuthorId = authorId,
            ProductId = productId
        };
        
        ticket.AddDomainEvent(new ServiceDeskSystem.Domain.Events.TicketCreatedEvent(ticket.Id, authorId, title));
        return ticket;
    }

    public void ChangeStatus(TicketStatus newStatus, int? actorUserId)
    {
        if (this.Status == newStatus) return;
        
        var oldStatus = this.Status;
        this.Status = newStatus;
        
        this.AddDomainEvent(new ServiceDeskSystem.Domain.Events.TicketStatusChangedEvent(this.Id, oldStatus, newStatus, actorUserId));
    }
}
