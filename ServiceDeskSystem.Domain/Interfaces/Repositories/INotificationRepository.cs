using ServiceDeskSystem.Domain.Entities;
namespace ServiceDeskSystem.Domain.Interfaces;
public interface INotificationRepository : IRepository<Notification> {
    Task<List<Notification>> GetRecentForUserAsync(int userId, int take);
    Task<int> GetUnreadCountAsync(int userId);
    Task<List<Notification>> GetUnreadForUserAsync(int userId);
    Task<Notification?> GetByIdAndUserAsync(int id, int userId);
}
