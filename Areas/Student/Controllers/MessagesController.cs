using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Student;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "STUDENT")]
    public class MessagesController : Controller
    {
        private readonly IStudentMessageService _messageService;

        public MessagesController(IStudentMessageService messageService)
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

        // GET: /Student/Messages
        public async Task<IActionResult> Index(int? teacherId)
        {
            int studentId = GetCurrentUserId();
            if (studentId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            var chatSummaries = await _messageService.GetTeacherChatSummariesAsync(studentId);

            // If no teacherId is selected, default to the first teacher if available
            if (!teacherId.HasValue && chatSummaries.Count > 0)
            {
                teacherId = chatSummaries[0].TeacherId;
            }

            ViewBag.ChatSummaries = chatSummaries;
            ViewBag.SelectedTeacherId = teacherId;

            var messages = new List<ChatMessage>();
            if (teacherId.HasValue)
            {
                var selectedTeacher = await _messageService.GetTeacherByIdAsync(teacherId.Value);
                if (selectedTeacher != null)
                {
                    ViewBag.SelectedTeacherName = selectedTeacher.FullName;
                    ViewBag.SelectedTeacherAvatar = string.IsNullOrWhiteSpace(selectedTeacher.AvatarUrl)
                        || selectedTeacher.AvatarUrl.Contains("/default-images/avatar.png", StringComparison.OrdinalIgnoreCase)
                        || selectedTeacher.AvatarUrl.Contains("/images/default-avatar.png", StringComparison.OrdinalIgnoreCase)
                        ? "/images/default-avatar.svg"
                        : selectedTeacher.AvatarUrl;
                    ViewBag.SelectedTeacherEmail = selectedTeacher.Email;

                    messages = await _messageService.GetConversationAsync(studentId, teacherId.Value);
                }
            }

            return View(messages);
        }

        // POST: /Student/Messages/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int receiverId, string messageText)
        {
            int studentId = GetCurrentUserId();
            if (studentId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            if (!string.IsNullOrWhiteSpace(messageText))
            {
                await _messageService.SendMessageAsync(studentId, receiverId, messageText);
            }

            return RedirectToAction(nameof(Index), new { teacherId = receiverId });
        }
    }
}
