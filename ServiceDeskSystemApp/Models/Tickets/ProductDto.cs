using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Tickets;

public class ProductDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
