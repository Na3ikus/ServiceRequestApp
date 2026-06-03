using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Infrastructure.Data.Repository
{
    public class NotificationRepository : TemplateRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(BugTrackerDbContext context) : base(context) {}

        protected override DbSet<Notification> DbSet => this.Context.Notifications;

        public async Task<List<Notification>> GetRecentForUserAsync(int userId, int take) {
            return await this.Context.Notifications
                .AsNoTracking()
                .Include(n => n.ActorUser)
                .Where(n => n.RecipientUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        public async Task<int> GetUnreadCountAsync(int userId) {
            return await this.Context.Notifications
                .AsNoTracking()
                .CountAsync(n => n.RecipientUserId == userId && !n.IsRead)
                .ConfigureAwait(false);
        }
        public async Task<List<Notification>> GetUnreadForUserAsync(int userId) {
            return await this.Context.Notifications
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        public async Task<Notification?> GetByIdAndUserAsync(int id, int userId) {
            return await this.Context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.RecipientUserId == userId)
                .ConfigureAwait(false);
        }
    }
}
