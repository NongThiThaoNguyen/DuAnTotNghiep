using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces;

public interface IAdminChatMonitorService
{
    Task<ChatStatsViewModel> GetChatStatsAsync();
    Task<List<TeacherStudentChatRowViewModel>> GetTeacherStudentChatsAsync();
    Task<List<AiTutorSessionRowViewModel>> GetAiTutorSessionsAsync();
}
