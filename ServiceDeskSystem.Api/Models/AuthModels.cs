using System.ComponentModel;

namespace ServiceDeskSystem.Api.Models;

public sealed class LoginRequest
{
    [DefaultValue("")]
    public string Username { get; set; } = string.Empty;

    [DefaultValue("")]
    public string Password { get; set; } = string.Empty;
}

public sealed class RegisterRequest
{
    [DefaultValue("")]
    public string Username { get; set; } = string.Empty;

    [DefaultValue("")]
    public string Password { get; set; } = string.Empty;

    [DefaultValue("")]
    public string FirstName { get; set; } = string.Empty;

    [DefaultValue("")]
    public string LastName { get; set; } = string.Empty;

    [DefaultValue("")]
    public string? Email { get; set; }
}

public sealed record LoginResponse(string Token, UserDto User);

public sealed record UserDto(int Id, string Login, string Role, bool IsActive, string? FirstName, string? LastName);
