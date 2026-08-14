using ServiceDeskSystem.Domain.Common;

namespace ServiceDeskSystem.Domain.Entities;

public class WorkLog : Entity
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public int TimeSpentMinutes { get; set; }

    public DateTime DateLogged { get; set; }

    public string Description { get; set; } = string.Empty;
}
