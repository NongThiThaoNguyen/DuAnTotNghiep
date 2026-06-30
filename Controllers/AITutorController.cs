using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.ViewModels.AILearn;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Controllers
{
    [Authorize]
    public class AITutorController : Controller
    {
        private readonly IAiTutorService _aiTutorService;
        private readonly ApplicationDbContext _context;

        public AITutorController(IAiTutorService aiTutorService, ApplicationDbContext context)
        {
            _aiTutorService = aiTutorService;
            _context = context;
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

        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            // Get or Create Conversation
            var conv = await _aiTutorService.GetOrCreateConversationAsync(userId);

            // Get Messages
            var messages = await _aiTutorService.GetMessagesAsync(conv.Id);

            var vm = new ChatViewModel
            {
                ConversationId = conv.Id,
                Messages = messages
            };

            var studentUser = await _context.Users.FindAsync(userId);
            ViewBag.StudentAvatarUrl = studentUser?.AvatarUrl ?? "/default-images/avatar.png";

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int conversationId, string messageText)
        {
            int userId = GetCurrentUserId();
            if (userId == 0 || string.IsNullOrWhiteSpace(messageText))
            {
                return Json(new { success = false, message = "Yêu cầu không hợp lệ." });
            }

            var conv = await _context.AiTutorConversations.FindAsync(conversationId);
            if (conv == null || conv.StudentId != userId)
            {
                return Json(new { success = false, message = "Không tìm thấy hội thoại." });
            }

            try
            {
                string replyText = await _aiTutorService.SendMessageAndGetReplyAsync(conversationId, userId, messageText);
                return Json(new { success = true, replyText = replyText });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi kết nối AI Tutor: {ex.Message}" });
            }
        }
    }
}
