using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class AiTutorConversation
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int? TopicId { get; set; }

    public int? LearningPathNodeId { get; set; }

    public string? Title { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AiTutorMessage> AiTutorMessages { get; set; } = new List<AiTutorMessage>();

    public virtual LearningPathNode? LearningPathNode { get; set; }

    public virtual User Student { get; set; } = null!;

    public virtual LearningTopic? Topic { get; set; }
}
