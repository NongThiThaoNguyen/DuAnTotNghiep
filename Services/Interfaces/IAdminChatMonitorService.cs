using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces;

public interface IAdminChatMonitorService
{
    Task<ChatStatsViewModel> GetChatStatsAsync();
    Task<List<TeacherStudentChatRowViewModel>> GetTeacherStudentChatsAsync(string? search = null, int? teacherId = null, int? classroomId = null, string? status = null, DateTime? from = null, DateTime? to = null);
    Task<List<AdminChatMessageViewModel>> GetConversationAsync(int teacherId, int studentId, string? search = null);
    Task<List<AiTutorSessionRowViewModel>> GetAiTutorSessionsAsync();
}
