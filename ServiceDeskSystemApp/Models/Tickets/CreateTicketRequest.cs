using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Tickets;

public class CreateTicketRequest
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public TicketPriority Priority { get; set; }

    [JsonPropertyName("ticketType")]
    public TicketType TicketType { get; set; }

    [JsonPropertyName("productId")]
    public int? ProductId { get; set; }

    [JsonPropertyName("tagIds")]
    public List<int>? TagIds { get; set; }
}
