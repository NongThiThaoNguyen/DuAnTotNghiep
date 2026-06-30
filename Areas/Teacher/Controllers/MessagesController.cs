using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
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

        public class StudentChatSummary
        {
            public int StudentId { get; set; }
            public string StudentName { get; set; } = "";
            public string StudentAvatar { get; set; } = "";
            public int UnreadCount { get; set; }
            public string LastMessageText { get; set; } = "";
            public DateTime? LastMessageTime { get; set; }
        }

        // GET: Teacher/Messages
        public async Task<IActionResult> Index(int? studentId)
        {
            int teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            // 1. Get all students
            var students = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE")
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var chatSummaries = new List<StudentChatSummary>();

            foreach (var student in students)
            {
                // Unread Count sent by this student to this teacher
                int unread = await _context.ChatMessages
                    .CountAsync(m => m.SenderId == student.Id && m.ReceiverId == teacherId && !m.IsRead);

                // Last Message
                var lastMsg = await _context.ChatMessages
                    .Where(m => (m.SenderId == teacherId && m.ReceiverId == student.Id) || (m.SenderId == student.Id && m.ReceiverId == teacherId))
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();

                chatSummaries.Add(new StudentChatSummary
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    StudentAvatar = student.AvatarUrl ?? "/default-images/avatar.png",
                    UnreadCount = unread,
                    LastMessageText = lastMsg?.MessageText ?? "Chưa có tin nhắn mới.",
                    LastMessageTime = lastMsg?.CreatedAt
                });
            }

            ViewBag.ChatSummaries = chatSummaries;
            ViewBag.SelectedStudentId = studentId;

            var messages = new List<ChatMessage>();
            if (studentId.HasValue)
            {
                var selectedStudent = await _context.Users.FindAsync(studentId.Value);
                ViewBag.SelectedStudentName = selectedStudent?.FullName ?? "Học viên";
                ViewBag.SelectedStudentAvatar = selectedStudent?.AvatarUrl ?? "/default-images/avatar.png";

                // Mark messages from student to teacher as read
                var unreadMsgs = await _context.ChatMessages
                    .Where(m => m.SenderId == studentId.Value && m.ReceiverId == teacherId && !m.IsRead)
                    .ToListAsync();
                
                if (unreadMsgs.Any())
                {
                    foreach (var m in unreadMsgs)
                    {
                        m.IsRead = true;
                    }
                    await _context.SaveChangesAsync();
                }

                // Get conversation thread
                messages = await _context.ChatMessages
                    .Where(m => (m.SenderId == teacherId && m.ReceiverId == studentId.Value) || (m.SenderId == studentId.Value && m.ReceiverId == teacherId))
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();
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

            if (!string.IsNullOrWhiteSpace(messageText))
            {
                var msg = new ChatMessage
                {
                    SenderId = teacherId,
                    ReceiverId = receiverId,
                    MessageText = messageText.Trim(),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ChatMessages.Add(msg);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { studentId = receiverId });
        }
    }
}
