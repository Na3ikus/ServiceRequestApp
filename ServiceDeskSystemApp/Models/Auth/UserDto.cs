using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Auth;

public class UserDto
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
}
