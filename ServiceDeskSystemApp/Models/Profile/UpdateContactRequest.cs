using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Profile;

public class UpdateContactRequest
{
    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("company")]
    public string? Company { get; set; }

    [JsonPropertyName("department")]
    public string? Department { get; set; }
}
