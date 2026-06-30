using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.StudyPlan
{
    public class MonthlyStudyPlanViewModel
    {
        public string PathTitle { get; set; } = "";
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthLabel { get; set; } = "";
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalMinutes { get; set; }
        public double CompletionPercent => TotalTasks > 0 ? Math.Round(CompletedTasks * 100.0 / TotalTasks, 1) : 0;
        public List<WeekSummaryViewModel> WeekSummaries { get; set; } = new();
        public bool HasActivePath { get; set; }
    }

    public class WeekSummaryViewModel
    {
        public int WeekNumber { get; set; }
        public DateOnly WeekStart { get; set; }
        public DateOnly WeekEnd { get; set; }
        public int TaskCount { get; set; }
        public int CompletedCount { get; set; }
        public int TotalMinutes { get; set; }
        public List<DailyStudyTaskViewModel> Tasks { get; set; } = new();
    }
}
