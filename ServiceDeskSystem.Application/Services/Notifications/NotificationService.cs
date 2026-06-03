using ServiceDeskSystem.Application.Services.Notifications.Models;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Application.Services.Notifications;

public sealed class NotificationService(
    IRepositoryFacadeFactory repositoryFacadeFactory,
    IRealtimeNotifier realtimeNotifier) : INotificationService
{
    public async Task<IReadOnlyList<UserNotificationDto>> GetRecentForUserAsync(int userId, int take = 10)
    {
        if (userId <= 0)
        {
            return [];
        }

        try
        {
            await using var repo = repositoryFacadeFactory.Create();

            var notifications = await repo.Notifications.GetRecentForUserAsync(userId, Math.Max(1, take)).ConfigureAwait(false);
            return notifications
                .Select(n => new UserNotificationDto(
                    n.Id,
                    n.TicketId,
                    n.Type,
                    n.Message,
                    n.ActorUser?.Login,
                    n.CreatedAt,
                    n.IsRead))
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    public async Task DeleteAsync(int notificationId, int userId)
    {
        if (notificationId <= 0 || userId <= 0)
        {
            return;
        }

        try
        {
            await using var repo = repositoryFacadeFactory.Create();
            var notification = await repo.Notifications.GetByIdAndUserAsync(notificationId, userId).ConfigureAwait(false);

            if (notification is null)
            {
                return;
            }

            await repo.Notifications.DeleteAsync(notification.Id).ConfigureAwait(false);
            await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            await realtimeNotifier.NotifyNotificationsChangedAsync([userId]).ConfigureAwait(false);
        }
        catch
        {
            // Notifications are best-effort and must not break user actions.
        }
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        if (userId <= 0)
        {
            return 0;
        }

        try
        {
            await using var repo = repositoryFacadeFactory.Create();
            return await repo.Notifications.GetUnreadCountAsync(userId).ConfigureAwait(false);
        }
        catch
        {
            return 0;
        }
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        if (userId <= 0)
        {
            return;
        }

        try
        {
            await using var repo = repositoryFacadeFactory.Create();
            var items = await repo.Notifications.GetUnreadForUserAsync(userId).ConfigureAwait(false);

            if (items.Count == 0)
            {
                return;
            }

            foreach (var notification in items)
            {
                notification.IsRead = true;
            }

            await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            await realtimeNotifier.NotifyNotificationsChangedAsync([userId]).ConfigureAwait(false);
        }
        catch
        {
            // Notifications are best-effort and must not break user actions.
        }
    }

    public async Task CreateCommentNotificationAsync(int ticketId, int commentAuthorId)
    {
        try
        {
            await using var repo = repositoryFacadeFactory.Create();

            var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

            if (ticket is null)
            {
                return;
            }

            var actor = await repo.Users.GetByIdAsync(commentAuthorId).ConfigureAwait(false);

            var recipients = new[] { ticket.AuthorId, ticket.DeveloperId }
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Where(id => id != commentAuthorId)
                .Distinct()
                .ToList();

            if (recipients.Count == 0)
            {
                return;
            }

            var message = $"{actor?.Login ?? "Someone"} replied in ticket #{ticket.Id}";

            foreach (var recipientId in recipients)
            {
                await repo.Notifications.CreateAsync(new Notification
                {
                    RecipientUserId = recipientId,
                    ActorUserId = commentAuthorId,
                    TicketId = ticket.Id,
                    Type = "CommentAdded",
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                }).ConfigureAwait(false);
            }

            await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            await realtimeNotifier.NotifyNotificationsChangedAsync(recipients).ConfigureAwait(false);
        }
        catch
        {
            // Notifications are best-effort and must not break user actions.
        }
    }

    public async Task CreateStatusChangedNotificationAsync(int ticketId, TicketStatus oldStatus, TicketStatus newStatus, int? actorUserId)
    {
        try
        {
            await using var repo = repositoryFacadeFactory.Create();

            var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

            if (ticket is null)
            {
                return;
            }

            string actorLogin = "Someone";
            if (actorUserId.HasValue)
            {
                var actor = await repo.Users.GetByIdAsync(actorUserId.Value).ConfigureAwait(false);
                actorLogin = actor?.Login ?? "Someone";
            }

            var notification = new Notification
            {
                RecipientUserId = ticket.AuthorId,
                ActorUserId = actorUserId,
                TicketId = ticket.Id,
                Type = "StatusChanged",
                Message = $"{actorLogin} changed ticket #{ticket.Id} status: {oldStatus} → {newStatus}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            };

            await repo.Notifications.CreateAsync(notification).ConfigureAwait(false);
            await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            await realtimeNotifier.NotifyNotificationsChangedAsync([ticket.AuthorId]).ConfigureAwait(false);
        }
        catch
        {
            // Notifications are best-effort and must not break user actions.
        }
    }

    public async Task CreateDatesChangedNotificationAsync(int ticketId, int? actorUserId)
    {
        try
        {
            await using var repo = repositoryFacadeFactory.Create();

            var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

            if (ticket is null)
            {
                return;
            }

            // Don't notify the actor themselves
            if (actorUserId == ticket.AuthorId)
            {
                return;
            }

            string actorLogin = "Someone";
            if (actorUserId.HasValue)
            {
                var actor = await repo.Users.GetByIdAsync(actorUserId.Value).ConfigureAwait(false);
                actorLogin = actor?.Login ?? "Someone";
            }

            var notification = new Notification
            {
                RecipientUserId = ticket.AuthorId,
                ActorUserId = actorUserId,
                TicketId = ticket.Id,
                Type = "DatesChanged",
                Message = $"{actorLogin} updated the schedule for ticket #{ticket.Id}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            };

            await repo.Notifications.CreateAsync(notification).ConfigureAwait(false);
            await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            await realtimeNotifier.NotifyNotificationsChangedAsync([ticket.AuthorId]).ConfigureAwait(false);
        }
        catch
        {
            // Notifications are best-effort and must not break user actions.
        }
    }
}

