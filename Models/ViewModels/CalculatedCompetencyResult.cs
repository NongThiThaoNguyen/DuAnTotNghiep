using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels
{
    // ============================================================
    //  CalculatedCompetencyResult
    //  Đây là OUTPUT duy nhất của CompetencyScoreCalculatorService.
    //  Nó đóng vai trò "tài liệu đầu vào chuẩn" (Structured Context)
    //  cho tầng xây dựng Prompt AI ở Task 4.
    // ============================================================

    /// <summary>
    /// Kết quả tổng hợp sau khi áp dụng toàn bộ logic Rule-based
    /// lên dữ liệu thô từ AssessmentInputDto.
    /// Chứa đầy đủ thông tin định lượng + xếp hạng ưu tiên để
    /// tầng AI có thể đưa ra nhận xét định tính chính xác.
    /// </summary>
    public class CalculatedCompetencyResult
    {
        // ── Metadata định danh ──────────────────────────────────────

        /// <summary>ID của lần thử bài test gốc.</summary>
        public int AttemptId { get; set; }

        /// <summary>ID học sinh.</summary>
        public int StudentId { get; set; }

        /// <summary>Tên bài Placement Test.</summary>
        public string TestTitle { get; set; } = null!;

        /// <summary>Thời điểm tính toán (UTC).</summary>
        public DateTime CalculatedAt { get; set; }

        // ── Điểm tổng quan ──────────────────────────────────────────

        /// <summary>Tổng điểm học sinh đạt được.</summary>
        public decimal TotalEarnedScore { get; set; }

        /// <summary>Tổng điểm tối đa của bài test.</summary>
        public decimal TotalMaxScore { get; set; }

        /// <summary>
        /// Tỷ lệ % chính xác tổng thể = (TotalEarnedScore / TotalMaxScore) * 100.
        /// Bằng 0 nếu TotalMaxScore = 0 (tránh DivisionByZero).
        /// </summary>
        public decimal OverallAccuracyPercentage { get; set; }

        /// <summary>Ước lượng trình độ CEFR sơ bộ từ điểm tổng thể (Rule-based).</summary>
        public string EstimatedCefrLevel { get; set; } = null!;

        // ── Phân tích theo Skill ─────────────────────────────────────

        /// <summary>
        /// Danh sách kết quả chi tiết theo từng kỹ năng tiếng Anh.
        /// Mỗi phần tử đã được đánh dấu IsWeakness / IsStrength.
        /// </summary>
        public List<SkillCompetencyScore> SkillScores { get; set; } = new List<SkillCompetencyScore>();

        // ── Phân tích theo Topic ─────────────────────────────────────

        /// <summary>
        /// Danh sách kết quả chi tiết theo từng chủ đề học.
        /// Đã được sắp xếp từ yếu nhất → mạnh nhất (AccuracyPercentage tăng dần).
        /// </summary>
        public List<TopicCompetencyScore> TopicScores { get; set; } = new List<TopicCompetencyScore>();

        // ── Phân tích theo Cấp độ khó ────────────────────────────────

        /// <summary>
        /// Tỷ lệ chính xác ở từng cấp độ: BASIC, MEDIUM, ADVANCED.
        /// Dùng để xác định học sinh bị "gãy" ở phân khúc nào.
        /// </summary>
        public List<DifficultyBreakdown> DifficultyBreakdowns { get; set; } = new List<DifficultyBreakdown>();

        /// <summary>
        /// Chẩn đoán pattern học lực dựa trên difficulty breakdown.
        /// Ví dụ: "BASIC_GAP", "ADVANCED_GAP", "CONSISTENT_LOW", "CONSISTENT_HIGH".
        /// </summary>
        public string KnowledgeGapPattern { get; set; } = null!;

        // ── Danh sách Topic ưu tiên ──────────────────────────────────

        /// <summary>
        /// Top 3–5 Topic cần tập trung xử lý GẤP, đã được tính
        /// bằng thuật toán kết hợp: AccuracyPercentage thấp +
        /// trọng số ưu tiên từ Onboarding (nếu có).
        /// </summary>
        public List<PriorityTopicItem> PriorityTopics { get; set; } = new List<PriorityTopicItem>();

        // ── Cảnh báo xử lý ──────────────────────────────────────────

        /// <summary>
        /// Danh sách cảnh báo phát sinh trong quá trình tính toán.
        /// Ví dụ: skill không có câu hỏi, onboarding chưa hoàn thành.
        /// </summary>
        public List<string> CalculationWarnings { get; set; } = new List<string>();

        // ── Dữ liệu câu sai ────────────────────────────────────────────

        /// <summary>
        /// Danh sách câu hỏi học sinh trả lời sai (pass-through từ Task 2).
        /// Được đưa trực tiếp vào Prompt AI ở Task 4 để AI phân tích lỗi sai cụ thể.
        /// </summary>
        public List<WrongAnswerDto> WrongAnswers { get; set; } = new List<WrongAnswerDto>();
    }

    // ============================================================
    //  SkillCompetencyScore
    // ============================================================

    /// <summary>
    /// Kết quả phân tích năng lực của một kỹ năng tiếng Anh cụ thể.
    /// </summary>
    public class SkillCompetencyScore
    {
        /// <summary>ID kỹ năng trong CSDL.</summary>
        public int SkillId { get; set; }

        /// <summary>Mã kỹ năng (LISTENING, READING, WRITING, SPEAKING, GRAMMAR, VOCABULARY...).</summary>
        public string SkillCode { get; set; } = null!;

        /// <summary>Tên kỹ năng hiển thị.</summary>
        public string SkillName { get; set; } = null!;

        /// <summary>Tổng số câu hỏi thuộc kỹ năng này trong bài test.</summary>
        public int TotalQuestions { get; set; }

        /// <summary>Số câu trả lời đúng.</summary>
        public int CorrectAnswers { get; set; }

        /// <summary>Điểm học sinh đạt được cho kỹ năng này.</summary>
        public decimal EarnedScore { get; set; }

        /// <summary>Điểm tối đa của kỹ năng này trong bài test.</summary>
        public decimal MaxScore { get; set; }

        /// <summary>
        /// Tỷ lệ % chính xác = (CorrectAnswers / TotalQuestions) * 100.
        /// Bằng 0 khi TotalQuestions = 0.
        /// </summary>
        public decimal AccuracyPercentage { get; set; }

        /// <summary>
        /// True nếu AccuracyPercentage dưới 50%.
        /// Đây là kỹ năng học sinh đang yếu, cần ưu tiên củng cố.
        /// </summary>
        public bool IsWeakness { get; set; }

        /// <summary>
        /// True nếu AccuracyPercentage trên 80%.
        /// Đây là kỹ năng học sinh đang mạnh, có thể học nâng cao.
        /// </summary>
        public bool IsStrength { get; set; }

        /// <summary>
        /// Nhãn định tính Rule-based: "WEAK" | "DEVELOPING" | "STRONG".
        /// Giúp AI dễ diễn đạt hơn trong prompt.
        /// </summary>
        public string PerformanceLabel { get; set; } = null!;
    }

    // ============================================================
    //  TopicCompetencyScore
    // ============================================================

    /// <summary>
    /// Kết quả phân tích năng lực của một chủ đề học cụ thể.
    /// </summary>
    public class TopicCompetencyScore
    {
        /// <summary>ID chủ đề trong CSDL.</summary>
        public int TopicId { get; set; }

        /// <summary>Mã chủ đề.</summary>
        public string TopicCode { get; set; } = null!;

        /// <summary>Tiêu đề chủ đề.</summary>
        public string TopicTitle { get; set; } = null!;

        /// <summary>ID kỹ năng mà chủ đề này thuộc về.</summary>
        public int SkillId { get; set; }

        /// <summary>Mã kỹ năng cha, dùng để nhóm khi render.</summary>
        public string SkillCode { get; set; } = null!;

        /// <summary>Tổng số câu hỏi của topic này trong bài test.</summary>
        public int TotalQuestions { get; set; }

        /// <summary>Số câu trả lời đúng.</summary>
        public int CorrectAnswers { get; set; }

        /// <summary>Điểm học sinh đạt được.</summary>
        public decimal EarnedScore { get; set; }

        /// <summary>Điểm tối đa.</summary>
        public decimal MaxScore { get; set; }

        /// <summary>
        /// Tỷ lệ % chính xác = (CorrectAnswers / TotalQuestions) * 100.
        /// Bằng 0 khi TotalQuestions = 0.
        /// </summary>
        public decimal AccuracyPercentage { get; set; }

        /// <summary>
        /// Điểm ưu tiên cuối cùng sau khi đã tích hợp trọng số Onboarding.
        /// Giá trị càng thấp = cần ưu tiên học càng sớm.
        /// Công thức: PriorityScore = AccuracyPercentage - (OnboardingWeight * 20).
        /// </summary>
        public decimal PriorityScore { get; set; }

        /// <summary>
        /// Nhãn định tính: "CRITICAL" (< 40%) | "WEAK" (40–59%) | "DEVELOPING" (60–79%) | "STRONG" (>= 80%).
        /// </summary>
        public string PerformanceLabel { get; set; } = null!;
    }

    // ============================================================
    //  DifficultyBreakdown
    // ============================================================

    /// <summary>
    /// Thống kê tỷ lệ chính xác theo cấp độ khó.
    /// </summary>
    public class DifficultyBreakdown
    {
        /// <summary>Cấp độ: BASIC | MEDIUM | ADVANCED.</summary>
        public string DifficultyLevel { get; set; } = null!;

        /// <summary>Tổng số câu của cấp độ này trong bài test.</summary>
        public int TotalQuestions { get; set; }

        /// <summary>Số câu đúng.</summary>
        public int CorrectAnswers { get; set; }

        /// <summary>Điểm đạt được.</summary>
        public decimal EarnedScore { get; set; }

        /// <summary>Điểm tối đa.</summary>
        public decimal MaxScore { get; set; }

        /// <summary>Tỷ lệ % chính xác theo cấp độ này.</summary>
        public decimal AccuracyPercentage { get; set; }

        /// <summary>
        /// True nếu tỷ lệ chính xác ở cấp này dưới 50%,
        /// tức là học sinh đang bị "gãy" ở phân khúc này.
        /// </summary>
        public bool IsBreakPoint { get; set; }
    }

    // ============================================================
    //  PriorityTopicItem
    // ============================================================

    /// <summary>
    /// Một topic nằm trong danh sách cần học ưu tiên.
    /// </summary>
    public class PriorityTopicItem
    {
        /// <summary>Thứ hạng ưu tiên (1 = quan trọng nhất).</summary>
        public int Rank { get; set; }

        /// <summary>ID chủ đề.</summary>
        public int TopicId { get; set; }

        /// <summary>Mã chủ đề.</summary>
        public string TopicCode { get; set; } = null!;

        /// <summary>Tiêu đề chủ đề.</summary>
        public string TopicTitle { get; set; } = null!;

        /// <summary>Mã kỹ năng cha.</summary>
        public string SkillCode { get; set; } = null!;

        /// <summary>Tỷ lệ % chính xác của học sinh ở topic này.</summary>
        public decimal AccuracyPercentage { get; set; }

        /// <summary>Điểm ưu tiên tổng hợp (càng thấp = càng cần học gấp).</summary>
        public decimal PriorityScore { get; set; }

        /// <summary>Mã lý do ưu tiên hệ thống.</summary>
        public string ReasonCode { get; set; } = null!;

        /// <summary>
        /// Lý do ưu tiên (do AI/Rule tạo ra, ví dụ:
        /// "Tỷ lệ đúng thấp (23%) + Onboarding ưu tiên kỹ năng GRAMMAR").
        /// </summary>
        public string PriorityReason { get; set; } = null!;

        /// <summary>Đã đáp ứng đủ điều kiện tiên quyết.</summary>
        public bool PrerequisitesMet { get; set; } = true;
    }
}
