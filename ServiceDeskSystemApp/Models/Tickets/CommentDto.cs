using System;
using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Tickets;

public class CommentDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("ticketId")]
    public int TicketId { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("createdById")]
    public int CreatedById { get; set; }

    [JsonPropertyName("createdByName")]
    public string CreatedByName { get; set; } = string.Empty;

    [JsonPropertyName("isInternal")]
    public bool IsInternal { get; set; }
}
