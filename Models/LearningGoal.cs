using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class LearningGoal
{
    public int Id { get; set; }

    public string GoalCode { get; set; } = null!;

    public string GoalName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<LearningPathTemplate> LearningPathTemplates { get; set; } = new List<LearningPathTemplate>();

    public virtual ICollection<StudentLearningPath> StudentLearningPaths { get; set; } = new List<StudentLearningPath>();

    public virtual ICollection<StudentLearningProfile> StudentLearningProfiles { get; set; } = new List<StudentLearningProfile>();
}
