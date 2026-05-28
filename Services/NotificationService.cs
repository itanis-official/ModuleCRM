using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Services
{
    public interface INotificationService
    {
        Task NotifyAsync(int? userId, string type, string title, string message, string? link = null);
        Task NotifyAllAsync(string type, string title, string message, string? link = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly CrmDbContext _db;

        public NotificationService(CrmDbContext db)
        {
            _db = db;
        }

        public async Task NotifyAsync(int? userId, string type, string title, string message, string? link = null)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                Link = link,
                CreatedAt = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
        }

        public Task NotifyAllAsync(string type, string title, string message, string? link = null)
            => NotifyAsync(null, type, title, message, link);
    }
}
