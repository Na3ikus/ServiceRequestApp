namespace ServiceDeskSystem.Data.Entities;

internal class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CurrentVersion { get; set; } = string.Empty;

    public int TechStackId { get; set; }

    public TechStack TechStack { get; set; } = null!;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
