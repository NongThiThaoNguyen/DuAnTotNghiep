using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class LearningObjective
{
    public int Id { get; set; }

    public int TopicId { get; set; }

    public string ObjectiveText { get; set; } = null!;

    public string CognitiveLevel { get; set; } = null!;

    public int OrderIndex { get; set; }

    public virtual LearningTopic Topic { get; set; } = null!;
}
