using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Tickets;

public class TicketDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public TicketStatus Status { get; set; }

    [JsonPropertyName("priority")]
    public TicketPriority Priority { get; set; }

    [JsonPropertyName("ticketType")]
    public TicketType TicketType { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("assignedToId")]
    public int? AssignedToId { get; set; }

    [JsonPropertyName("assignedToName")]
    public string? AssignedToName { get; set; }

    [JsonPropertyName("createdById")]
    public int CreatedById { get; set; }

    [JsonPropertyName("createdByName")]
    public string CreatedByName { get; set; } = string.Empty;

    [JsonPropertyName("product")]
    public ProductDto? Product { get; set; }

    [JsonPropertyName("tags")]
    public List<TagDto> Tags { get; set; } = new();
}
