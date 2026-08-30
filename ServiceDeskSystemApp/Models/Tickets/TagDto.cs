using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Tickets;

public class TagDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("colorCode")]
    public string? ColorCode { get; set; }
}
