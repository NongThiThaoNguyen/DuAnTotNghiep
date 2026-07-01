using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITeacherMessageService
    {
        Task<List<StudentChatSummaryViewModel>> GetStudentChatSummariesAsync(int teacherId);
        Task<User?> GetStudentByIdAsync(int studentId);
        Task<List<ChatMessage>> GetConversationAsync(int teacherId, int studentId);
        Task SendMessageAsync(int senderId, int receiverId, string messageText);
    }
}
