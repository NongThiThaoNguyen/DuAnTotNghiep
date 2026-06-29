using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class AiTutorMessage
{
    public long Id { get; set; }

    public int ConversationId { get; set; }

    public string SenderType { get; set; } = null!;

    public string MessageText { get; set; } = null!;

    public string? AiModel { get; set; }

    public int? TokenUsage { get; set; }

    public string? SafetyFlag { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AiTutorConversation Conversation { get; set; } = null!;
}
