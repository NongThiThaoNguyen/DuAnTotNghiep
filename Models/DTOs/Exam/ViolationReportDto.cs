using System;

namespace DuAnTotNghiep.Models.DTOs.Exam
{
    public class ViolationReportDto
    {
        public int AttemptId { get; set; }
        public string AttemptType { get; set; } = "QUIZ"; // "PLACEMENT_TEST", "QUIZ", "LEVEL_UP"
        public string ViolationType { get; set; } = "FULLSCREEN_EXIT"; // "FULLSCREEN_EXIT", "TAB_SWITCH", "WINDOW_BLUR"
        public int FullscreenExitCount { get; set; }
        public int TabSwitchCount { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
