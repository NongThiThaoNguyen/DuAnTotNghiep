using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetNotificationsAsync(int userId, bool? unreadOnly = null)
        {
            var query = _context.Notifications
                .Include(n => n.NotificationReads)
                .Where(n => n.TargetUserId == userId || n.TargetUserId == null);

            if (unreadOnly == true)
            {
                query = query.Where(n => !n.NotificationReads.Any(r => r.UserId == userId));
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .Include(n => n.NotificationReads)
                .Where(n => (n.TargetUserId == userId || n.TargetUserId == null) && !n.NotificationReads.Any(r => r.UserId == userId))
                .CountAsync();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            var exists = await _context.NotificationReads
                .AnyAsync(r => r.NotificationId == notificationId && r.UserId == userId);

            if (!exists)
            {
                var read = new NotificationRead
                {
                    NotificationId = notificationId,
                    UserId = userId,
                    ReadAt = DateTime.UtcNow
                };
                await _context.NotificationReads.AddAsync(read);
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}
