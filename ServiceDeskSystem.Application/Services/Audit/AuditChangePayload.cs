using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ServiceDeskSystem.Application.Services.Audit;

/// <summary>
/// Structured payload stored within AuditLog.Changes JSON string.
/// </summary>
public sealed class AuditChangePayload
{
    public string? Summary { get; set; }

    public string? Severity { get; set; } // "Info", "Warning", "Critical"

    public string? IpAddress { get; set; }

    public List<AuditDiffItem>? Diff { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        });
    }

    public static AuditChangePayload? TryParse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var trimmed = json.Trim();
        if (!trimmed.StartsWith('{') || !trimmed.EndsWith('}'))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AuditChangePayload>(trimmed, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Represents a field-level difference between old and new state.
/// </summary>
public sealed class AuditDiffItem
{
    public string Field { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public AuditDiffItem()
    {
    }

    public AuditDiffItem(string field, string? oldValue, string? newValue)
    {
        this.Field = field;
        this.OldValue = oldValue;
        this.NewValue = newValue;
    }
}
