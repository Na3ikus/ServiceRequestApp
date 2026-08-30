using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Common;

public class ApiErrorResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("errors")]
    public Dictionary<string, string[]>? Errors { get; set; }
}
