namespace ServiceDeskSystem.Data.Entities;

internal class TechStack
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
