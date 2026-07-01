using System;
using System.Collections.Generic;
using System.Linq;

namespace DuAnTotNghiep.Models.ViewModels.StudyPlan
{
    public class WeeklyStudyPlanViewModel
    {
        public string PathTitle { get; set; } = "";
        public DateOnly WeekStart { get; set; }
        public DateOnly WeekEnd { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int TotalMinutes { get; set; }
        public double CompletionPercent => TotalTasks > 0 ? Math.Round(CompletedTasks * 100.0 / TotalTasks, 1) : 0;
        public List<DayGroupViewModel> DayGroups { get; set; } = new();
        public bool HasActivePath { get; set; }
    }

    public class DayGroupViewModel
    {
        public DateOnly Date { get; set; }
        public string DayLabel { get; set; } = "";
        public bool IsToday { get; set; }
        public List<DailyStudyTaskViewModel> Tasks { get; set; } = new();
        public int TotalMinutes => Tasks.Sum(t => t.EstimatedMinutes);
    }
}
