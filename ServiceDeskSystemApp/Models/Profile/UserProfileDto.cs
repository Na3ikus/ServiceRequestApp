using System.Text.Json.Serialization;
using ServiceDeskSystemApp.Models.Auth;

namespace ServiceDeskSystemApp.Models.Profile;

public class UserProfileDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public UserRole Role { get; set; }

    [JsonPropertyName("contactInfo")]
    public ContactInfoDto? ContactInfo { get; set; }
}
