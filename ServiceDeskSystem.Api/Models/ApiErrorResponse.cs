namespace ServiceDeskSystem.Api.Models;

public sealed record ApiErrorResponse(
    int StatusCode,
    string Message,
    string? Details = null);
