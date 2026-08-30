using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Common;

public class PagedResult<T>
{
    [JsonPropertyName("items")]
    public IEnumerable<T> Items { get; set; } = new List<T>();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}
