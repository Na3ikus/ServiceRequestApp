namespace ServiceDeskSystem.Domain.Entities;

using ServiceDeskSystem.Domain.Common;

public class Tag : Entity
{
    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = "#3B82F6";

    public string? Description { get; set; }

    public int? CreatedById { get; set; }

    public User? CreatedBy { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
