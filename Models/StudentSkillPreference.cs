using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class StudentSkillPreference
{
    public int Id { get; set; }

    public int StudentProfileId { get; set; }

    public string SkillCode { get; set; } = null!;

    public int PriorityLevel { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual StudentLearningProfile StudentProfile { get; set; } = null!;
}
