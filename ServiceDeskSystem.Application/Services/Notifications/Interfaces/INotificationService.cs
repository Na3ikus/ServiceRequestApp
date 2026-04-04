using ServiceDeskSystem.Application.Services.Notifications.Models;

namespace ServiceDeskSystem.Application.Services.Notifications.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<UserNotificationDto>> GetRecentForUserAsync(int userId, int take = 10);

    Task<int> GetUnreadCountAsync(int userId);

    Task MarkAllAsReadAsync(int userId);

    Task DeleteAsync(int notificationId, int userId);

    Task CreateCommentNotificationAsync(int ticketId, int commentAuthorId);

    Task CreateStatusChangedNotificationAsync(int ticketId, string oldStatus, string newStatus, int? actorUserId);
}
