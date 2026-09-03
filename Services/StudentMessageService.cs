using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Student;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services
{
    public class StudentMessageService : IStudentMessageService
    {
        private readonly ApplicationDbContext _context;

        public StudentMessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TeacherChatSummaryViewModel>> GetTeacherChatSummariesAsync(int studentId)
        {
            // 1. Find teachers from classes student is enrolled in
            var enrolledClasses = await _context.Enrollments
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Teacher)
                .Where(e => e.StudentId == studentId && e.Status == "ACTIVE")
                .ToListAsync();

            var teacherClassMap = enrolledClasses
                .Where(e => e.Classroom?.Teacher != null)
                .GroupBy(e => e.Classroom.TeacherId)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(", ", g.Select(e => e.Classroom.ClassName).Distinct())
                );

            // 2. Find all active teachers
            var teachers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleCode == "TEACHER" && u.Status == "ACTIVE")
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // 3. Get unread message counts for this student from each teacher
            var unreadCounts = await _context.ChatMessages
                .Where(m => m.ReceiverId == studentId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                .Select(g => new { TeacherId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.TeacherId, g => g.Count);

            // 4. Get last messages exchanged with each teacher
            var lastMessagesQuery = await _context.ChatMessages
                .Where(m => m.SenderId == studentId || m.ReceiverId == studentId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var lastMessages = lastMessagesQuery
                .GroupBy(m => m.SenderId == studentId ? m.ReceiverId : m.SenderId)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault());

            var chatSummaries = new List<TeacherChatSummaryViewModel>();

            foreach (var teacher in teachers)
            {
                int unread = unreadCounts.TryGetValue(teacher.Id, out var count) ? count : 0;
                lastMessages.TryGetValue(teacher.Id, out var lastMsg);
                teacherClassMap.TryGetValue(teacher.Id, out var className);

                // Only show teachers who teach the student OR have message history OR show all teachers if none
                bool isRelevant = teacherClassMap.ContainsKey(teacher.Id) || lastMsg != null;

                chatSummaries.Add(new TeacherChatSummaryViewModel
                {
                    TeacherId = teacher.Id,
                    TeacherName = teacher.FullName,
                    TeacherEmail = teacher.Email,
                    TeacherAvatar = NormalizeAvatarUrl(teacher.AvatarUrl),
                    ClassName = className,
                    UnreadCount = unread,
                    LastMessageText = lastMsg?.MessageText ?? (className != null ? $"Lớp: {className}" : "Chưa có tin nhắn mới."),
                    LastMessageTime = lastMsg?.CreatedAt
                });
            }

            // Order: Has unread messages first, then recent messages, then class teachers, then alphabetical
            return chatSummaries
                .OrderByDescending(s => s.UnreadCount > 0)
                .ThenByDescending(s => s.LastMessageTime.HasValue)
                .ThenByDescending(s => s.LastMessageTime)
                .ThenByDescending(s => !string.IsNullOrEmpty(s.ClassName))
                .ThenBy(s => s.TeacherName)
                .ToList();
        }

        private static string NormalizeAvatarUrl(string? avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(avatarUrl)
                || avatarUrl.Contains("/default-images/avatar.png", StringComparison.OrdinalIgnoreCase)
                || avatarUrl.Contains("/images/default-avatar.png", StringComparison.OrdinalIgnoreCase))
            {
                return "/images/default-avatar.svg";
            }

            return avatarUrl;
        }

        public async Task<User?> GetTeacherByIdAsync(int teacherId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == teacherId && u.Role.RoleCode == "TEACHER");
        }

        public async Task<List<ChatMessage>> GetConversationAsync(int studentId, int teacherId)
        {
            var unreadMsgs = await _context.ChatMessages
                .Where(m => m.SenderId == teacherId && m.ReceiverId == studentId && !m.IsRead)
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
                .Where(m => (m.SenderId == studentId && m.ReceiverId == teacherId) || (m.SenderId == teacherId && m.ReceiverId == studentId))
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
