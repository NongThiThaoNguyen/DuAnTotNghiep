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
    }
}
