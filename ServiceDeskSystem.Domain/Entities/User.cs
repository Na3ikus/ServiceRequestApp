using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Common;

public class User : AggregateRoot
{
    public string Login { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    public int PersonId { get; set; }

    public Person Person { get; set; } = null!;

    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Notification> NotificationsReceived { get; set; } = new List<Notification>();

    public ICollection<Notification> NotificationsCreated { get; set; } = new List<Notification>();
}

