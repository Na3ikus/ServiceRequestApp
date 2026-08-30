using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Tickets;

public class TicketStatsDto
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("open")]
    public int Open { get; set; }

    [JsonPropertyName("critical")]
    public int Critical { get; set; }
}
