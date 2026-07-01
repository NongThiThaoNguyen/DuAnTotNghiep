using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.StudyPlan
{
    public class TodayStudyPlanViewModel
    {
        public string PathTitle { get; set; } = "";
        public int TotalMinutesToday { get; set; }
        public int DailyLimitMinutes { get; set; }
        public bool IsOverloaded => TotalMinutesToday > DailyLimitMinutes && DailyLimitMinutes > 0;
        public List<DailyStudyTaskViewModel> TodayTasks { get; set; } = new();
        public List<DailyStudyTaskViewModel> OverdueTasks { get; set; } = new();
        public DailyStudyTaskViewModel? ContinueTask { get; set; }
        public DailyStudyTaskViewModel? NextTask { get; set; }
        public bool HasActivePath { get; set; }
    }
}
