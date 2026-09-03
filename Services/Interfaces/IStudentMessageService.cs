using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Student;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IStudentMessageService
    {
        Task<List<TeacherChatSummaryViewModel>> GetTeacherChatSummariesAsync(int studentId);
        Task<User?> GetTeacherByIdAsync(int teacherId);
        Task<List<ChatMessage>> GetConversationAsync(int studentId, int teacherId);
        Task SendMessageAsync(int senderId, int receiverId, string messageText);
    }
}
