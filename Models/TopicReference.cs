using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class TopicReference
{
    public int Id { get; set; }

    public int TopicId { get; set; }

    public int ReferenceSourceId { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ReferenceSource ReferenceSource { get; set; } = null!;

    public virtual LearningTopic Topic { get; set; } = null!;
}
