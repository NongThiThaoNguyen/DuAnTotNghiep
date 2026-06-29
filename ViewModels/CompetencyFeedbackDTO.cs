using System.Collections.Generic;

namespace DuAnTotNghiep.ViewModels
{
    /// <summary>
    /// DTO tổng hợp toàn bộ nội dung feedback của 1 báo cáo năng lực.
    /// Dùng trực tiếp làm Model cho Razor View báo cáo kết quả.
    /// </summary>
    public class CompetencyFeedbackDTO
    {
        // ── Section 1: Dashboard Summary ─────────────────────────────
        public int AnalysisId { get; set; }
        public string DashboardSummary { get; set; } = "";
        public string CurrentLevelName { get; set; } = "";
        public string RecommendedLevelName { get; set; } = "";
        public decimal ConfidenceScore { get; set; }

        // Flag cảnh báo khi dữ liệu bài thi quá ít
        public bool IsDataThinWarning { get; set; }
        public string DataThinMessage { get; set; } = "";

        // ── Section 2: Strengths ──────────────────────────────────────
        public List<FeedbackItemDTO> Strengths { get; set; } = new();

        // ── Section 3: Weaknesses gắn với Topic ──────────────────────
        public List<FeedbackItemDTO> Weaknesses { get; set; } = new();

        // ── Section 4: Recommended Actions / Gaps ────────────────────
        public List<RecommendedActionDTO> RecommendedActions { get; set; } = new();
    }

    /// <summary>
    /// Một mục trong danh sách Strength hoặc Weakness.
    /// </summary>
    public class FeedbackItemDTO
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        // Giới hạn mô tả để View không bị tràn chữ
        public string ShortDescription => Description.Length > 120
            ? Description.Substring(0, 120) + "..."
            : Description;
    }

    /// <summary>
    /// Một hành động khuyến nghị gắn với Skill hoặc Topic cụ thể.
    /// </summary>
    public class RecommendedActionDTO
    {
        public string ActionText { get; set; } = "";
        public string? SkillName { get; set; }
        public string? TopicTitle { get; set; }
        public int? TopicId { get; set; }
        public int Priority { get; set; }
    }
}
