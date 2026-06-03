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

    [System.Text.Json.Serialization.JsonIgnore]
    public Person Person { get; set; } = null!;

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Notification> NotificationsReceived { get; set; } = new List<Notification>();

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Notification> NotificationsCreated { get; set; } = new List<Notification>();
}
