using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.AILearn;

namespace DuAnTotNghiep.Controllers;

[Authorize]
public class AITutorController : Controller
{
    private readonly ApplicationDbContext _context;

    public AITutorController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int userId))
        {
            return userId;
        }
        return 0;
    }

    public async Task<IActionResult> Index()
    {
        int userId = GetCurrentUserId();
        if (userId == 0) return RedirectToAction("Login", "Account");

        // Get or Create Conversation
        var conv = await _context.AiTutorConversations
            .Include(c => c.AiTutorMessages)
            .FirstOrDefaultAsync(c => c.StudentId == userId);

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
            _context.AiTutorConversations.Add(conv);
            await _context.SaveChangesAsync();
        }

        var messages = conv.AiTutorMessages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageViewModel
            {
                Id = m.Id,
                SenderType = m.SenderType.ToUpper(),
                MessageText = m.MessageText,
                CreatedAt = m.CreatedAt
            }).ToList();

        var vm = new ChatViewModel
        {
            ConversationId = conv.Id,
            Messages = messages
        };

        ViewBag.StudentAvatarUrl = (await _context.Users.FindAsync(userId))?.AvatarUrl ?? "/default-images/avatar.png";

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(int conversationId, string messageText)
    {
        int userId = GetCurrentUserId();
        if (userId == 0 || string.IsNullOrWhiteSpace(messageText))
        {
            return Json(new { success = false, message = "Yêu cầu không hợp lệ." });
        }

        var conv = await _context.AiTutorConversations.FindAsync(conversationId);
        if (conv == null || conv.StudentId != userId)
        {
            return Json(new { success = false, message = "Không tìm thấy hội thoại." });
        }

        // 1. Save Student Message
        var studentMsg = new AiTutorMessage
        {
            ConversationId = conversationId,
            SenderType = "STUDENT",
            MessageText = messageText.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _context.AiTutorMessages.Add(studentMsg);
        await _context.SaveChangesAsync();

        // 2. Generate Simulated bot Response based on message content
        string replyText = GetSimulatedTutorReply(messageText);

        // 3. Save AI Message
        var aiMsg = new AiTutorMessage
        {
            ConversationId = conversationId,
            SenderType = "AI",
            MessageText = replyText,
            AiModel = "gemini-1.5-pro",
            TokenUsage = 180,
            CreatedAt = DateTime.UtcNow
        };
        _context.AiTutorMessages.Add(aiMsg);
        await _context.SaveChangesAsync();

        // 4. Save Activity Log
        var log = new StudyActivityLog
        {
            StudentId = userId,
            ActivityType = "CHAT",
            CreatedAt = DateTime.UtcNow,
            DurationMinutes = 2
        };
        _context.StudyActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        return Json(new { success = true, replyText = replyText });
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
