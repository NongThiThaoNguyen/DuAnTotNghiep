using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotificationsAsync(int userId, bool? unreadOnly = null);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);

        // Admin Methods
        Task<List<Notification>> GetAllAsync(int page, int pageSize);
        Task<Notification?> GetByIdAsync(int id);
        Task<int> GetTotalCountAsync();
        Task CreateAsync(Notification notification);
        Task DeleteAsync(int id);
        Task SendToAllStudentsAsync(string title, string content, string type, int createdBy);
        Task SendToUserAsync(string title, string content, string type, int targetUserId, int createdBy);
    }
}
