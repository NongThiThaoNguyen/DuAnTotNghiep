using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsApiController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsApiController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] bool? unreadOnly = null)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var notifications = await _notificationService.GetNotificationsAsync(userId, unreadOnly);
            
            var result = notifications.Select(n => new
            {
                id = n.Id,
                title = n.Title,
                content = n.Content,
                notificationType = n.NotificationType,
                createdAt = n.CreatedAt,
                timeAgo = GetTimeAgo(n.CreatedAt),
                isRead = n.NotificationReads.Any(r => r.UserId == userId)
            });

            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            int count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            bool success = await _notificationService.MarkAsReadAsync(id, userId);
            int unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { success, unreadCount });
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            bool success = await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { success, unreadCount = 0 });
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;
            if (span.TotalMinutes < 1) return "Vừa xong";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays} ngày trước";
            return dateTime.ToString("dd/MM/yyyy HH:mm");
        }
    }
}
