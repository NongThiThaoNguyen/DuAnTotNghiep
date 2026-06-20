using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class StudentLearningProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? CurrentLevelId { get; set; }

    public int? TargetLevelId { get; set; }

    public int? MainGoalId { get; set; }

    public decimal? TargetScore { get; set; }

    public int? DailyStudyMinutes { get; set; }

    public int? WeeklyStudyDays { get; set; }

    public string? PreferredStudyTime { get; set; }

    public string? LearningNote { get; set; }

    public string OnboardingStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual EnglishProficiencyLevel? CurrentLevel { get; set; }

    public virtual LearningGoal? MainGoal { get; set; }

    public virtual ICollection<StudentSkillPreference> StudentSkillPreferences { get; set; } = new List<StudentSkillPreference>();

    public virtual EnglishProficiencyLevel? TargetLevel { get; set; }

    public virtual User User { get; set; } = null!;
}
