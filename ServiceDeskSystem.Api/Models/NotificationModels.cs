namespace ServiceDeskSystem.Api.Models;

public sealed record SendTestEmailRequest(string ToEmail, string? Subject = null);
