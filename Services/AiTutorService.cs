using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.AILearn;
using DuAnTotNghiep.Services.AI;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class AiTutorService : IAiTutorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAIProvider _aiProvider;
        private readonly IConfiguration _config;
        private readonly ILogger<AiTutorService> _logger;

        public AiTutorService(
            ApplicationDbContext context,
            IAIProvider aiProvider,
            IConfiguration config,
            ILogger<AiTutorService> logger)
        {
            _context = context;
            _aiProvider = aiProvider;
            _config = config;
            _logger = logger;
        }

        public async Task<AiTutorConversation> GetOrCreateConversationAsync(int userId)
        {
            var conv = await _context.AiTutorConversations
                .Include(c => c.AiTutorMessages)
                .FirstOrDefaultAsync(c => c.StudentId == userId && c.Status == "ACTIVE");

            if (conv == null)
            {
                conv = new AiTutorConversation
                {
                    StudentId = userId,
                    Title = "Conversation with AI Tutor",
                    Status = "ACTIVE",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.AiTutorConversations.AddAsync(conv);
                await _context.SaveChangesAsync();
            }

            return conv;
        }

        public async Task<List<ChatMessageViewModel>> GetMessagesAsync(int conversationId)
        {
            return await _context.AiTutorMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessageViewModel
                {
                    Id = m.Id,
                    SenderType = m.SenderType.ToUpper(),
                    MessageText = m.MessageText,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<string> SendMessageAndGetReplyAsync(int conversationId, int userId, string message)
        {
            // 1. Save Student Message
            var studentMsg = new AiTutorMessage
            {
                ConversationId = conversationId,
                SenderType = "STUDENT",
                MessageText = message.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            await _context.AiTutorMessages.AddAsync(studentMsg);
            await _context.SaveChangesAsync();

            // 2. Build system prompt with student level context
            var profile = await _context.StudentLearningProfiles
                .Include(p => p.CurrentLevel)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);
            
            var levelStr = profile?.CurrentLevel?.Name ?? profile?.CurrentLevel?.Code ?? "Beginner";
            var systemPrompt = $"You are an English tutor. The student's English level is {levelStr}. Respond in Vietnamese, but provide English examples where appropriate. Keep responses educational, helpful, and friendly.";

            // 3. Build past messages context (last 10 messages)
            var pastMessages = await _context.AiTutorMessages
                .Where(m => m.ConversationId == conversationId && m.Id != studentMsg.Id)
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .ToListAsync();
            pastMessages.Reverse();

            var contextBuilder = new System.Text.StringBuilder();
            foreach (var msg in pastMessages)
            {
                contextBuilder.AppendLine($"{msg.SenderType}: {msg.MessageText}");
            }
            contextBuilder.AppendLine($"STUDENT: {message}");
            var userPrompt = contextBuilder.ToString();

            // 4. Call AI Provider or Fallback
            string replyText = "";
            bool isFallback = false;

            var apiKey = _config["AI:ApiKey"] ?? _config["OpenAI:ApiKey"];
            var isGemini = apiKey?.StartsWith("AIzaSy") == true || 
                           (_config["AI:Endpoint"]?.Contains("generativelanguage") == true);
            var targetModel = isGemini ? (_config["AI:Model"] ?? "gemini-2.5-flash") : (_config["AI:Model"] ?? "gpt-4o-mini");

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                try
                {
                    replyText = await _aiProvider.GenerateAsync(systemPrompt, userPrompt, "CHAT", null, userId, targetModel);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AI generation failed. Falling back to offline simulated reply.");
                    replyText = GetSimulatedTutorReply(message);
                    isFallback = true;
                }
            }
            else
            {
                replyText = GetSimulatedTutorReply(message);
                isFallback = true;
            }

            // 5. Save AI response
            var aiMsg = new AiTutorMessage
            {
                ConversationId = conversationId,
                SenderType = "AI",
                MessageText = replyText,
                AiModel = isFallback ? (isGemini ? "gemini-1.5-flash (Offline)" : "gpt-4o-mini (Offline)") : targetModel,
                TokenUsage = isFallback ? 0 : 180,
                CreatedAt = DateTime.UtcNow
            };
            await _context.AiTutorMessages.AddAsync(aiMsg);

            // 6. Save Activity Log
            var log = new StudyActivityLog
            {
                StudentId = userId,
                ActivityType = "CHAT",
                CreatedAt = DateTime.UtcNow,
                DurationMinutes = 2
            };
            await _context.StudyActivityLogs.AddAsync(log);

            await _context.SaveChangesAsync();

            return replyText;
        }

        public async Task<int> GetConversationCountAsync(int userId)
        {
            return await _context.AiTutorConversations.CountAsync(c => c.StudentId == userId);
        }

        private string GetSimulatedTutorReply(string query)
        {
            query = query.ToLower();

            if (query.Contains("thì") || query.Contains("tenses") || query.Contains("ngữ pháp"))
            {
                return "Trong tiếng Anh, ngữ pháp về các thì (Tenses) là vô cùng quan trọng.\n\n" +
                       "Ví dụ về **thì Hiện tại hoàn thành (Present Perfect)**:\n" +
                       "- Công thức: `S + have/has + V3/ed`\n" +
                       "- Dùng để nói về một hành động đã xảy ra trong quá khứ nhưng kết quả vẫn liên quan đến hiện tại.\n" +
                       "- *Example:* I have lived in Hanoi for 3 years (Tôi đã sống ở HN được 3 năm - hiện tại vẫn đang sống ở đây).\n\n" +
                       "Bạn có muốn mình đưa thêm các bài tập nhỏ để luyện tập không?";
            }
            else if (query.Contains("nói") || query.Contains("phản xạ") || query.Contains("giao tiếp"))
            {
                return "Để luyện phản xạ nói giao tiếp hàng ngày, phương pháp tốt nhất là thực hành hội thoại theo ngữ cảnh.\n\n" +
                       "Hãy thử trả lời câu hỏi này của mình bằng tiếng Anh nhé:\n" +
                       "*\"What is your favorite hobby, and how often do you do it?\"*\n\n" +
                       "Cố gắng dùng các cụm từ mình đề xuất như *'I am keen on...'* hoặc *'I'm into...'* nhé!";
            }
            else if (query.Contains("email") || query.Contains("look forward to"))
            {
                return "Cấu trúc `look forward to` rất phổ biến trong email công việc để thể hiện sự lịch sự.\n\n" +
                       "Quy tắc quan trọng: **Sau 'look forward to' luôn đi kèm V-ing hoặc Danh từ**.\n" +
                       "- *Lưu ý sai:* I look forward to hear from you. (Sai)\n" +
                       "- *Chỉnh sửa đúng:* I look forward to **hearing** from you. (Đúng)\n\n" +
                       "Bạn có muốn thử viết một câu kết thư email công việc ngắn để mình sửa giúp không?";
            }
            else if (query.Contains("keen on") || query.Contains("enjoy"))
            {
                return "Đây là câu hỏi rất hay về sắc thái từ vựng!\n\n" +
                       "- **Enjoy + V-ing**: Mang nghĩa thích thú, tận hưởng một hoạt động thông thường. (Ví dụ: I enjoy watching movies).\n" +
                       "- **Be keen on + V-ing/Noun**: Mang nghĩa đam mê, thiết tha, vô cùng hào hứng với điều gì đó, sắc thái mạnh mẽ hơn enjoy. (Ví dụ: I am keen on exploring new technologies).\n\n" +
                       "Bạn thích dùng cấu trúc nào hơn khi nói về sở thích của mình?";
            }
            else if (query.Contains("xin chào") || query.Contains("hi") || query.Contains("hello"))
            {
                return "Xin chào! Mình là AI Tutor, trợ lý học tập cá nhân của bạn. Hôm nay bạn muốn tìm hiểu hay ôn tập kiến thức nào? Mình sẵn sàng hỗ trợ bạn!";
            }

            return "Cảm ơn câu hỏi của bạn! Đó là một chủ đề học tập rất thú vị. Bạn có thể nói chi tiết hơn hoặc đưa ra ví dụ cụ thể để mình giải thích và hướng dẫn rõ hơn cho bạn nhé!";
        }
    }
}
