using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Tickets;

public class CreateCommentRequest
{
    [JsonPropertyName("ticketId")]
    public int TicketId { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("isInternal")]
    public bool IsInternal { get; set; }
}
