using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class MessagesController : Controller
    {
        private readonly ITeacherMessageService _messageService;

        public MessagesController(ITeacherMessageService messageService)
        {
            _messageService = messageService;
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

        // GET: Teacher/Messages
        public async Task<IActionResult> Index(int? studentId)
        {
            int teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            var chatSummaries = await _messageService.GetStudentChatSummariesAsync(teacherId);

            ViewBag.ChatSummaries = chatSummaries;
            ViewBag.SelectedStudentId = studentId;

            var messages = new List<ChatMessage>();
            if (studentId.HasValue)
            {
                var selectedStudent = await _messageService.GetStudentByIdAsync(studentId.Value);
                ViewBag.SelectedStudentName = selectedStudent?.FullName ?? "Học viên";
                ViewBag.SelectedStudentAvatar = selectedStudent?.AvatarUrl ?? "/default-images/avatar.png";

                messages = await _messageService.GetConversationAsync(teacherId, studentId.Value);
            }

            return View(messages);
        }

        // POST: Teacher/Messages/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int receiverId, string messageText)
        {
            int teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            await _messageService.SendMessageAsync(teacherId, receiverId, messageText);

            return RedirectToAction(nameof(Index), new { studentId = receiverId });
        }
    }
}
