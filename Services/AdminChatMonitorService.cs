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

    public async Task<List<TeacherStudentChatRowViewModel>> GetTeacherStudentChatsAsync()
    {
        var messages = await _context.ChatMessages.AsNoTracking()
            .Include(m => m.Sender)
                .ThenInclude(u => u.Role)
            .Include(m => m.Receiver)
                .ThenInclude(u => u.Role)
            .ToListAsync();

        return messages
            .Select(message => TryMapTeacherStudentMessage(message))
            .Where(row => row != null)
            .GroupBy(row => new { row!.TeacherId, row.StudentId })
            .Select(group => new TeacherStudentChatRowViewModel
            {
                TeacherId = group.Key.TeacherId,
                TeacherName = group.First()!.TeacherName,
                StudentId = group.Key.StudentId,
                StudentName = group.First()!.StudentName,
                MessageCount = group.Count(),
                LastMessageAt = group.Max(row => row!.LastMessageAt)
            })
            .OrderByDescending(row => row.LastMessageAt)
            .ToList();
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
