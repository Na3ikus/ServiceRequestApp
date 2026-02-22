namespace ServiceDeskSystem.Api.Models;

public sealed record LoginRequest(string Username, string Password);

public sealed record RegisterRequest(
    string Username,
    string Password,
    string FirstName,
    string LastName,
    string? Email);

public sealed record LoginResponse(string Token, UserDto User);

public sealed record UserDto(int Id, string Login, string Role, bool IsActive, string? FirstName, string? LastName);
