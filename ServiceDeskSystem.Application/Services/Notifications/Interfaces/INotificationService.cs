using ServiceDeskSystem.Application.Services.Notifications.Models;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Application.Services.Notifications.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<UserNotificationDto>> GetRecentForUserAsync(int userId, int take = 10);

    Task<int> GetUnreadCountAsync(int userId);

    Task MarkAllAsReadAsync(int userId);

    Task DeleteAsync(int notificationId, int userId);

    Task CreateCommentNotificationAsync(int ticketId, int commentAuthorId);

    Task CreateStatusChangedNotificationAsync(int ticketId, TicketStatus oldStatus, TicketStatus newStatus, int? actorUserId);

    Task CreateDatesChangedNotificationAsync(int ticketId, int? actorUserId);
}
