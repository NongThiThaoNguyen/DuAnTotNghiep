using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class TeacherMessageService : ITeacherMessageService
    {
        private readonly ApplicationDbContext _context;

        public TeacherMessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentChatSummaryViewModel>> GetStudentChatSummariesAsync(int teacherId)
        {
            var students = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE")
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // Get unread counts
            var unreadCounts = await _context.ChatMessages
                .Where(m => m.ReceiverId == teacherId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                .Select(g => new { SenderId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.SenderId, g => g.Count);

            // Get last messages
            var lastMessagesQuery = await _context.ChatMessages
                .Where(m => m.SenderId == teacherId || m.ReceiverId == teacherId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync(); // Pull to memory to group properly since EF might struggle with group by max date

            var lastMessages = lastMessagesQuery
                .GroupBy(m => m.SenderId == teacherId ? m.ReceiverId : m.SenderId)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault());

            var chatSummaries = new List<StudentChatSummaryViewModel>();

            foreach (var student in students)
            {
                int unread = unreadCounts.TryGetValue(student.Id, out var count) ? count : 0;
                lastMessages.TryGetValue(student.Id, out var lastMsg);

                chatSummaries.Add(new StudentChatSummaryViewModel
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    StudentAvatar = student.AvatarUrl ?? "/default-images/avatar.png",
                    UnreadCount = unread,
                    LastMessageText = lastMsg?.MessageText ?? "Chưa có tin nhắn mới.",
                    LastMessageTime = lastMsg?.CreatedAt
                });
            }

            return chatSummaries;
        }

        public async Task<User?> GetStudentByIdAsync(int studentId)
        {
            return await _context.Users.FindAsync(studentId);
        }

        public async Task<List<ChatMessage>> GetConversationAsync(int teacherId, int studentId)
        {
            var unreadMsgs = await _context.ChatMessages
                .Where(m => m.SenderId == studentId && m.ReceiverId == teacherId && !m.IsRead)
                .ToListAsync();

            if (unreadMsgs.Any())
            {
                foreach (var m in unreadMsgs)
                {
                    m.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            return await _context.ChatMessages
                .Where(m => (m.SenderId == teacherId && m.ReceiverId == studentId) || (m.SenderId == studentId && m.ReceiverId == teacherId))
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task SendMessageAsync(int senderId, int receiverId, string messageText)
        {
            if (!string.IsNullOrWhiteSpace(messageText))
            {
                var msg = new ChatMessage
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    MessageText = messageText.Trim(),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ChatMessages.Add(msg);
                await _context.SaveChangesAsync();
            }
        }
    }
}
