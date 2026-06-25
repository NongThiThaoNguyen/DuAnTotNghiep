using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class TopicPrerequisite
{
    public int Id { get; set; }

    public int TopicId { get; set; }

    public int PrerequisiteTopicId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual LearningTopic Topic { get; set; } = null!;

    public virtual LearningTopic PrerequisiteTopic { get; set; } = null!;
}
