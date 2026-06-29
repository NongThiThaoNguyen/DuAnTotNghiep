using DuAnTotNghiep.Models.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces
{
    /// <summary>
    /// Định nghĩa contract cho Service tính toán năng lực học sinh
    /// theo phương pháp Rule-based (thuần logic nghiệp vụ, không gọi AI).
    ///
    /// Đây là tầng M7.Task3 trong pipeline:
    ///   Task2 (AggregateTestData) → [AssessmentInputDto]
    ///   → Task3 (CalculateScores) → [CalculatedCompetencyResult]
    ///   → Task4 (BuildPrompt) → [AI Gemini]
    /// </summary>
    public interface ICompetencyScoreCalculatorService
    {
        /// <summary>
        /// Nhận dữ liệu thô đã chuẩn hóa từ Task 2 và thực hiện
        /// toàn bộ logic tính toán định lượng Rule-based:
        ///   1. Tính tỷ lệ % chính xác theo từng Skill.
        ///   2. Gắn nhãn IsWeakness / IsStrength cho Skill.
        ///   3. Tính tỷ lệ % chính xác theo từng Topic.
        ///   4. Phân tích tỷ lệ chính xác theo Cấp độ khó (BASIC/MEDIUM/ADVANCED).
        ///   5. Xác định KnowledgeGapPattern từ difficulty breakdown.
        ///   6. Xác định ước lượng CEFR sơ bộ từ điểm tổng thể.
        ///   7. Tạo danh sách 3–5 Topic ưu tiên học gấp (kết hợp onboarding preference).
        /// </summary>
        /// <param name="inputData">
        ///   Dữ liệu chuẩn hóa từ AssessmentInputDto (output của Task 2).
        ///   Không được null.
        /// </param>
        /// <param name="onboardingSkillPreferences">
        ///   Danh sách mã kỹ năng học sinh ưu tiên khai báo lúc Onboarding,
        ///   kèm theo mức độ ưu tiên (1 = cao nhất).
        ///   Truyền null hoặc danh sách rỗng nếu học sinh chưa hoàn thành Onboarding.
        /// </param>
        /// <returns>
        ///   CalculatedCompetencyResult chứa toàn bộ kết quả định lượng
        ///   đã được xử lý, sẵn sàng làm đầu vào cho Prompt AI ở Task 4.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   Ném ra khi inputData là null.
        /// </exception>
        CalculatedCompetencyResult CalculateScores(
            AssessmentInputDto inputData,
            IEnumerable<OnboardingSkillPreferenceDto>? onboardingSkillPreferences,
            IEnumerable<LearningTopicDto> topicCatalog);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Supporting DTO: OnboardingSkillPreferenceDto
    //  Mục đích: Truyền dữ liệu Onboarding vào Calculator mà không
    //  tạo dependency trực tiếp vào Domain Model.
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Khai báo ưu tiên kỹ năng của học sinh từ bước Onboarding.
    /// Dùng để tăng trọng số ưu tiên cho các Topic thuộc kỹ năng đó.
    /// </summary>
    public class OnboardingSkillPreferenceDto
    {
        /// <summary>
        /// Mã kỹ năng (SkillCode) học sinh muốn tập trung.
        /// Ví dụ: "GRAMMAR", "SPEAKING", "VOCABULARY".
        /// </summary>
        public string SkillCode { get; set; } = null!;

        /// <summary>
        /// Mức độ ưu tiên học sinh tự khai (1 = cao nhất).
        /// Dùng để cộng điểm ưu tiên cho topic thuộc kỹ năng này.
        /// </summary>
        public int PriorityLevel { get; set; }
    }
}
