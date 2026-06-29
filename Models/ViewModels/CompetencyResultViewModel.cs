using System;
using System.Collections.Generic;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.ViewModels
{
    /// <summary>
    /// ViewModel được làm sạch để gửi sang View cho học sinh và Admin/Giáo viên.
    /// Loại bỏ hoàn toàn các trường dữ liệu nội bộ của AI (như Token, Cost, Prompt thô)
    /// để đảm bảo trải nghiệm người dùng và an toàn bảo mật.
    /// </summary>
    public class CompetencyResultViewModel
    {
        public int AnalysisId { get; set; }
        public int StudentId { get; set; }

        public string StudentName { get; set; } = "Student";

        /// <summary>
        /// Thời điểm tính toán.
        /// </summary>
        public DateTime CalculatedAt { get; set; }

        /// <summary>
        /// Điểm tổng quan (tính theo %).
        /// </summary>
        public decimal OverallAccuracy { get; set; }

        /// <summary>
        /// Trình độ CEFR ước lượng bằng Rule-based.
        /// </summary>
        public string EstimatedCefrLevel { get; set; } = null!;

        /// <summary>
        /// Tên mức độ hiện tại.
        /// </summary>
        public string CurrentLevelName { get; set; } = "";

        /// <summary>
        /// Tên mức độ khuyến nghị tiếp theo.
        /// </summary>
        public string RecommendedLevelName { get; set; } = "";

        /// <summary>
        /// Đoạn tóm tắt tổng quan do AI viết.
        /// </summary>
        public string DashboardSummary { get; set; } = "";

        /// <summary>
        /// Trạng thái của bài phân tích (COMPLETED, PENDING, FAILED).
        /// </summary>
        public string Status { get; set; } = "COMPLETED";

        /// <summary>
        /// Cờ báo hiệu AI bị lỗi và hệ thống đang dùng fallback text (Rule-based).
        /// </summary>
        public bool IsAiFallback { get; set; }

        public List<FeedbackItemDTO> Strengths { get; set; } = new List<FeedbackItemDTO>();
        public List<FeedbackItemDTO> Weaknesses { get; set; } = new List<FeedbackItemDTO>();
        public List<RecommendedActionDTO> RecommendedActions { get; set; } = new List<RecommendedActionDTO>();
        public List<PriorityTopicItem> PriorityTopics { get; set; } = new List<PriorityTopicItem>();
    }
}
