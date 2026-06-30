using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.AILearn;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IAiTutorService
    {
        Task<AiTutorConversation> GetOrCreateConversationAsync(int userId);
        Task<List<ChatMessageViewModel>> GetMessagesAsync(int conversationId);
        Task<string> SendMessageAndGetReplyAsync(int conversationId, int userId, string message);
        Task<int> GetConversationCountAsync(int userId);
    }
}
