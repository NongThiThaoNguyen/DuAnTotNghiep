using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.AILearn;

public class StatisticsViewModel
{
    public int TotalStudyMinutes { get; set; }
    public int TotalLessonsCompleted { get; set; }
    public decimal AverageQuizAccuracy { get; set; }
    public int WeeklyStreak { get; set; }

    // Chart Data
    public List<int> WeeklyStudyMinutes { get; set; } = new() { 45, 60, 30, 90, 75, 40, 50 }; // Mon-Sun
    public List<string> WeeklyDaysLabel { get; set; } = new() { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };

    public List<double> SkillAverages { get; set; } = new() { 80, 70, 75, 65, 85 }; // Listening, Speaking, Reading, Writing, Grammar
    public List<string> SkillsLabel { get; set; } = new() { "Nghe hiểu", "Kỹ năng nói", "Đọc hiểu", "Viết luận", "Ngữ pháp" };
}
