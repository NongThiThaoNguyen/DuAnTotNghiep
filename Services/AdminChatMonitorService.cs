using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

public class AdminChatMonitorService : IAdminChatMonitorService
{
    private readonly ApplicationDbContext _context;

    public AdminChatMonitorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChatStatsViewModel> GetChatStatsAsync()
    {
        var teacherChats = await GetTeacherStudentChatsAsync();
        var aiSessions = await GetAiTutorSessionsAsync();

        return new ChatStatsViewModel
        {
            TeacherStudentConversationCount = teacherChats.Count,
            TeacherStudentMessageCount = teacherChats.Sum(c => c.MessageCount),
            AiTutorSessionCount = aiSessions.Count,
            AiTutorMessageCount = aiSessions.Sum(s => s.MessageCount),
            RecentTeacherChats = teacherChats.Take(8).ToList(),
            RecentAiSessions = aiSessions.Take(8).ToList()
        };
    }

    public async Task<List<TeacherStudentChatRowViewModel>> GetTeacherStudentChatsAsync(string? search = null, int? teacherId = null, int? classroomId = null, string? status = null, DateTime? from = null, DateTime? to = null)
    {
        var messages = await _context.ChatMessages.AsNoTracking()
            .Include(m => m.Sender)
                .ThenInclude(u => u.Role)
            .Include(m => m.Receiver)
                .ThenInclude(u => u.Role)
            .ToListAsync();

        var rows = messages
            .Select(message => TryMapTeacherStudentMessage(message))
            .Where(row => row != null)
            .GroupBy(row => new { row!.TeacherId, row.StudentId })
            .Select(group =>
            {
                var last = group.OrderByDescending(row => row!.LastMessageAt).First()!;
                var pairMessages = messages.Where(message =>
                    (message.SenderId == group.Key.TeacherId && message.ReceiverId == group.Key.StudentId) ||
                    (message.SenderId == group.Key.StudentId && message.ReceiverId == group.Key.TeacherId)).ToList();
                return new TeacherStudentChatRowViewModel
                {
                    TeacherId = group.Key.TeacherId,
                    TeacherName = last.TeacherName,
                    StudentId = group.Key.StudentId,
                    StudentName = last.StudentName,
                    MessageCount = pairMessages.Count,
                    LastMessageAt = pairMessages.Max(message => message.CreatedAt),
                    LastMessageText = pairMessages.OrderByDescending(message => message.CreatedAt).First().MessageText,
                    UnreadCount = pairMessages.Count(message => message.ReceiverId == group.Key.StudentId && !message.IsRead),
                    ClassNames = "Chưa xác định"
                };
            })
            .Where(row => (!teacherId.HasValue || row.TeacherId == teacherId.Value)
                && (!string.IsNullOrWhiteSpace(search) || true)
                && (string.IsNullOrWhiteSpace(status) || (status == "UNREAD" ? row.UnreadCount > 0 : row.UnreadCount == 0))
                && (!from.HasValue || row.LastMessageAt.Date >= from.Value.Date)
                && (!to.HasValue || row.LastMessageAt.Date <= to.Value.Date))
            .Where(row => string.IsNullOrWhiteSpace(search)
                || row.TeacherName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || row.StudentName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || row.LastMessageText.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.LastMessageAt)
            .ToList();

        if (classroomId.HasValue)
        {
            var studentIds = await _context.Enrollments.AsNoTracking()
                .Where(enrollment => enrollment.ClassroomId == classroomId.Value && enrollment.Status == "ACTIVE")
                .Select(enrollment => enrollment.StudentId).ToListAsync();
            rows = rows.Where(row => studentIds.Contains(row.StudentId)).ToList();
        }

        var rowStudentIds = rows.Select(row => row.StudentId).Distinct().ToList();
        var classRows = await _context.Enrollments.AsNoTracking()
            .Include(enrollment => enrollment.Classroom)
            .Where(enrollment => rowStudentIds.Contains(enrollment.StudentId) && enrollment.Status == "ACTIVE")
            .Select(enrollment => new { enrollment.StudentId, enrollment.Classroom.TeacherId, enrollment.Classroom.ClassName })
            .ToListAsync();
        foreach (var row in rows)
        {
            row.ClassNames = string.Join(", ", classRows
                .Where(classRow => classRow.StudentId == row.StudentId && classRow.TeacherId == row.TeacherId)
                .Select(classRow => classRow.ClassName).Distinct());
            if (string.IsNullOrWhiteSpace(row.ClassNames)) row.ClassNames = "Chưa xác định";
        }

        return rows;
    }

    public async Task<List<AdminChatMessageViewModel>> GetConversationAsync(int teacherId, int studentId, string? search = null)
    {
        var messages = await _context.ChatMessages.AsNoTracking()
            .Include(message => message.Sender).ThenInclude(sender => sender.Role)
            .Where(message => (message.SenderId == teacherId && message.ReceiverId == studentId)
                || (message.SenderId == studentId && message.ReceiverId == teacherId))
            .OrderBy(message => message.CreatedAt)
            .Select(message => new AdminChatMessageViewModel
            {
                Id = message.Id,
                SenderName = message.Sender.FullName,
                SenderRole = message.Sender.Role.RoleCode,
                MessageText = message.MessageText,
                CreatedAt = message.CreatedAt
            }).ToListAsync();
        return string.IsNullOrWhiteSpace(search)
            ? messages
            : messages.Where(message => message.MessageText.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<List<AiTutorSessionRowViewModel>> GetAiTutorSessionsAsync()
    {
        var sessions = await _context.AiTutorConversations.AsNoTracking()
            .Include(c => c.Student)
            .Include(c => c.Topic)
            .Include(c => c.AiTutorMessages)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();

        return sessions.Select(session => new AiTutorSessionRowViewModel
        {
            ConversationId = session.Id,
            StudentName = session.Student.FullName,
            TopicTitle = session.Topic?.Title ?? "Không gắn topic",
            Status = session.Status,
            MessageCount = session.AiTutorMessages.Count,
            UpdatedAt = session.UpdatedAt
        }).ToList();
    }

    private static TeacherStudentChatRowViewModel? TryMapTeacherStudentMessage(Models.ChatMessage message)
    {
        if (message.Sender.Role.RoleCode == "TEACHER" && message.Receiver.Role.RoleCode == "STUDENT")
        {
            return new TeacherStudentChatRowViewModel
            {
                TeacherId = message.SenderId,
                TeacherName = message.Sender.FullName,
                StudentId = message.ReceiverId,
                StudentName = message.Receiver.FullName,
                LastMessageAt = message.CreatedAt
            };
        }

        if (message.Sender.Role.RoleCode == "STUDENT" && message.Receiver.Role.RoleCode == "TEACHER")
        {
            return new TeacherStudentChatRowViewModel
            {
                TeacherId = message.ReceiverId,
                TeacherName = message.Receiver.FullName,
                StudentId = message.SenderId,
                StudentName = message.Sender.FullName,
                LastMessageAt = message.CreatedAt
            };
        }

        return null;
    }
}
