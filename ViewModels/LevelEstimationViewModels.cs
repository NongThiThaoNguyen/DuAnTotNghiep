namespace DuAnTotNghiep.ViewModels
{
    /// <summary>
    /// Kết quả ước lượng trình độ từ quy tắc Rule-based kết hợp AI suggestion.
    /// </summary>
    public class LevelEstimationResultDTO
    {
        public int StudentId { get; set; }
        public int AttemptId { get; set; }

        // Trình độ hiện tại ước lượng
        public int CurrentLevelId { get; set; }
        public string CurrentLevelCode { get; set; } = "";
        public string CurrentLevelName { get; set; } = "";

        // Trình độ khuyến nghị tiếp theo (target phù hợp thực tế)
        public int? RecommendedLevelId { get; set; }
        public string RecommendedLevelName { get; set; } = "";

        // Trình độ mục tiêu học sinh tự chọn (từ Onboarding M3)
        public int? OnboardingTargetLevelId { get; set; }
        public string OnboardingTargetLevelName { get; set; } = "";

        // Điểm tự tin – sẽ bị hạ nếu kỹ năng bị lệch quá nhiều
        public decimal ConfidenceScore { get; set; }

        // Điểm số tổng hợp từ AI đưa vào
        public decimal AiSuggestedScore { get; set; }

        // Cảnh báo nghịch lý: target < current
        public bool HasLevelParadoxWarning { get; set; }
        public string? LevelParadoxMessage { get; set; }

        // Cảnh báo kỹ năng lệch nhiều → confidence bị hạ
        public bool HasSkillImbalanceWarning { get; set; }
        public string? SkillImbalanceMessage { get; set; }

        // Target level từ M3 bị thiếu
        public bool HasNoTargetLevel { get; set; }

        // Disclaimer bắt buộc hiển thị trên View
        public string Disclaimer { get; set; } =
            "Đây là kết quả ước lượng dựa trên bài kiểm tra hệ thống, " +
            "không thay thế cho các chứng chỉ quốc tế chính thức.";
    }

    /// <summary>
    /// ViewModel phục vụ Razor View hiển thị Gap giữa trình độ hiện tại và mục tiêu.
    /// </summary>
    public class LevelGapViewModel
    {
        public int AnalysisId { get; set; }
        public int StudentId { get; set; }

        public string CurrentLevelName { get; set; } = "";
        public int CurrentOrderIndex { get; set; }

        public string? TargetLevelName { get; set; }
        public int? TargetOrderIndex { get; set; }

        // Số bậc cần leo để đạt mục tiêu (0 = đã đạt hoặc không có target)
        public int GapSteps { get; set; }

        // Phần trăm tiến trình (0–100) để vẽ progress bar trên View
        public int ProgressPercent { get; set; }

        public decimal ConfidenceScore { get; set; }

        public bool HasParadoxWarning { get; set; }
        public string? ParadoxMessage { get; set; }

        public string Disclaimer { get; set; } =
            "Đây là kết quả ước lượng dựa trên bài kiểm tra hệ thống, " +
            "không thay thế cho các chứng chỉ quốc tế chính thức.";
    }
}
