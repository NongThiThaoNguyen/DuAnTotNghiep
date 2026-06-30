using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.AILearn;

public class ChatViewModel
{
    public int ConversationId { get; set; }
    public List<ChatMessageViewModel> Messages { get; set; } = new();
    public List<string> SuggestedQuestions { get; set; } = new()
    {
        "Giải thích thì Hiện tại hoàn thành đơn giản hơn?",
        "Bài tập luyện phản xạ nói giao tiếp hàng ngày?",
        "Cách dùng 'look forward to' viết email chuyên nghiệp?",
        "Phân biệt 'keen on' và 'enjoy'?"
    };
}

public class ChatMessageViewModel
{
    public long Id { get; set; }
    public string SenderType { get; set; } = ""; // STUDENT or AI
    public string MessageText { get; set; } = "";
    public System.DateTime CreatedAt { get; set; }
    public string TimeLabel => CreatedAt.ToString("HH:mm");
}
