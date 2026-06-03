using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Application.Services.Notifications.Interfaces;
using ServiceDeskSystem.Application.Services.Notifications.Models;
using ServiceDeskSystem.Application.Services.Realtime.Interfaces;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Application.Services.Notifications;

public sealed class NotificationService(
    IDbContextFactory<BugTrackerDbContext> contextFactory,
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
            await using var context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);

            return await context.Notifications
                .AsNoTracking()
                .Where(n => n.RecipientUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(Math.Max(1, take))
                .Select(n => new UserNotificationDto(
                    n.Id,
                    n.TicketId,
                    n.Type,
                    n.Message,
                    n.ActorUser != null ? n.ActorUser.Login : null,
                    n.CreatedAt,
                    n.IsRead))
                .ToListAsync()
                .ConfigureAwait(false);
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
            await using var context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
            var notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId)
                .ConfigureAwait(false);

            if (notification is null)
            {
                return;
            }

            context.Notifications.Remove(notification);
            await context.SaveChangesAsync().ConfigureAwait(false);
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
            await using var context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
            return await context.Notifications
                .AsNoTracking()
                .CountAsync(n => n.RecipientUserId == userId && !n.IsRead)
                .ConfigureAwait(false);
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
            await using var context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
            var items = await context.Notifications
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .ToListAsync()
                .ConfigureAwait(false);

            if (items.Count == 0)
            {
                return;
            }

            foreach (var notification in items)
            {
                notification.IsRead = true;
            }

            await context.SaveChangesAsync().ConfigureAwait(false);
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
            await using var context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);

            var ticket = await context.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == ticketId)
                .ConfigureAwait(false);

            if (ticket is null)
            {
                return;
            }

            var actor = await context.Users
                .AsNoTracking()
                .Where(u => u.Id == commentAuthorId)
                .Select(u => u.Login)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

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

            var message = $"{actor ?? "Someone"} replied in ticket #{ticket.Id}";

            foreach (var recipientId in recipients)
            {
                context.Notifications.Add(new Notification
                {
                    RecipientUserId = recipientId,
                    ActorUserId = commentAuthorId,
                    TicketId = ticket.Id,
                    Type = "CommentAdded",
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                });
            }

            await context.SaveChangesAsync().ConfigureAwait(false);
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
            await using var context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);

            var ticket = await context.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == ticketId)
                .ConfigureAwait(false);

            if (ticket is null)
            {
                return;
            }

            var actorLogin = actorUserId is null
                ? "Someone"
                : await context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == actorUserId.Value)
                    .Select(u => u.Login)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false) ?? "Someone";

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

            context.Notifications.Add(notification);
            await context.SaveChangesAsync().ConfigureAwait(false);
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
            await using var context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);

            var ticket = await context.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == ticketId)
                .ConfigureAwait(false);

            if (ticket is null)
            {
                return;
            }

            // Don't notify the actor themselves
            if (actorUserId == ticket.AuthorId)
            {
                return;
            }

            var actorLogin = actorUserId is null
                ? "Someone"
                : await context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == actorUserId.Value)
                    .Select(u => u.Login)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false) ?? "Someone";

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

            context.Notifications.Add(notification);
            await context.SaveChangesAsync().ConfigureAwait(false);
            await realtimeNotifier.NotifyNotificationsChangedAsync([ticket.AuthorId]).ConfigureAwait(false);
        }
        catch
        {
            // Notifications are best-effort and must not break user actions.
        }
    }
}
