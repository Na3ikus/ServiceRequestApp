using System.Text.Json.Serialization;

namespace ServiceDeskSystemApp.Models.Auth;

public class LoginResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("user")]
    public UserDto? User { get; set; }
}
