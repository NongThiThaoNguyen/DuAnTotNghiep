using System.Collections.Generic;

namespace DuAnTotNghiep.ViewModels.Progress
{
    public class ProgressDashboardViewModel
    {
        public int StreakDays { get; set; }
        public int TotalStudyMinutes { get; set; }
        public int CompletedLessonsCount { get; set; }
        public int CompletedQuizzesCount { get; set; }

        // Progress of each skill (Listening, Speaking, Reading, Writing, Grammar, Vocabulary)
        public List<SkillProgressViewModel> SkillProgresses { get; set; } = new();

        // Weak areas identified (skills or topics with low average score)
        public List<TopicProgressViewModel> WeakTopics { get; set; } = new();

        // Improved areas identified
        public List<TopicProgressViewModel> ImprovedTopics { get; set; } = new();

        // Recent study activity logs
        public List<LearningHistoryItemViewModel> RecentActivities { get; set; } = new();
    }
}
