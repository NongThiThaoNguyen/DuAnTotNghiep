using System;

namespace DuAnTotNghiep.Helpers
{
    public static class StudyPlanHelper
    {
        public static string GetStatusLabel(string s) => s switch {
            "LOCKED" => "Chưa mở",
            "AVAILABLE" => "Sẵn sàng",
            "IN_PROGRESS" => "Đang học",
            "COMPLETED" => "Đã xong",
            "NEED_REVIEW" => "Cần ôn lại",
            "SKIPPED" => "Đã bỏ qua",
            _ => s
        };

        public static string GetStatusCssClass(string s, bool overdue) => s switch {
            "COMPLETED" => "task-completed",
            "IN_PROGRESS" => "task-in-progress",
            "NEED_REVIEW" => "task-need-review",
            "SKIPPED" => "task-skipped",
            "LOCKED" => "task-locked",
            _ => overdue ? "task-overdue" : "task-available"
        };

        public static bool IsOverdue(DateOnly? d, string s) =>
            d.HasValue && s is not ("COMPLETED" or "SKIPPED") && d.Value < DateOnly.FromDateTime(DateTime.Today);

        public static bool IsTodayTask(DateOnly? d, string s) =>
            d.HasValue && s is "AVAILABLE" or "IN_PROGRESS" && d.Value == DateOnly.FromDateTime(DateTime.Today);

        public static int GetDefaultMinutes(string t) => t switch {
            "LESSON" => 20,
            "QUIZ" => 15,
            "PRACTICE" => 25,
            "AI_TUTOR" => 15,
            "REVIEW" => 10,
            _ => 15
        };

        public static string GetViDayName(DayOfWeek d) => d switch {
            DayOfWeek.Monday => "Thứ Hai",
            DayOfWeek.Tuesday => "Thứ Ba",
            DayOfWeek.Wednesday => "Thứ Tư",
            DayOfWeek.Thursday => "Thứ Năm",
            DayOfWeek.Friday => "Thứ Sáu",
            DayOfWeek.Saturday => "Thứ Bảy",
            DayOfWeek.Sunday => "Chủ Nhật",
            _ => ""
        };
    }
}
