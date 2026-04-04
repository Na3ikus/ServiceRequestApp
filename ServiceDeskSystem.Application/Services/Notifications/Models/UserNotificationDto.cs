namespace ServiceDeskSystem.Application.Services.Notifications.Models;

public sealed record UserNotificationDto(
    int Id,
    int TicketId,
    string Type,
    string Message,
    string? ActorLogin,
    DateTime CreatedAt,
    bool IsRead);
