namespace ServiceDeskSystem.Domain.Entities;

public class TechStack
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
