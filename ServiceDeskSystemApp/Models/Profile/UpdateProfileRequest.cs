using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Profile;

public class UpdateProfileRequest
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;
}
