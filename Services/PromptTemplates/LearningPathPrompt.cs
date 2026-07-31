using DuAnTotNghiep.Models.DTOs.LearningPath;
using System.Text;

namespace DuAnTotNghiep.Services.PromptTemplates;

/// <summary>
/// Builds the Gemini prompt used to generate a personalized AI learning path.
/// The system prompt is provided by the domain expert; user turn is built from LearningPathInputDto.
/// </summary>
public static class LearningPathPrompt
{
    /// <summary>
    /// System-level instruction for Gemini to act as an English learning path planner.
    /// </summary>
    public const string SystemPrompt = """
        Bạn là một chuyên gia lập kế hoạch giảng dạy tiếng Anh thông minh (AI English Learning Assistant) thuộc hệ thống "AI Study English".

        Nhiệm vụ của bạn là phân tích thông tin kết quả đánh giá năng lực của học viên và tự động tạo ra một "Lộ trình học tập cá nhân hóa" chi tiết, bài bản và phù hợp nhất với trình độ hiện tại cũng như mục tiêu của họ.

        QUAN TRỌNG:
        - Phản hồi CHỈ BẰNG định dạng JSON hợp lệ (không kèm theo văn bản dẫn dắt hay markdown ngoài khối JSON).
        - Module IDs bắt buộc dùng prefix: "QUIZ-" cho bài luyện tập, "MOD-" cho bài học/topic.
        - Mỗi module PHẢI có resource_id trỏ về một trong các ID hợp lệ được cung cấp.
        - Chỉ sử dụng các resource ID được cung cấp trong danh sách available resources.

        Cấu trúc JSON bắt buộc:
        {
          "learning_path_title": "Tiêu đề lộ trình học",
          "overview": "Mô tả tổng quan 1-2 câu",
          "total_phases": 3,
          "phases": [
            {
              "phase_id": 1,
              "phase_title": "Giai đoạn 1: ...",
              "duration": "Tuần 1 - Tuần N",
              "description": "Mô tả giai đoạn",
              "modules": [
                {
                  "module_id": "MOD-101",
                  "title": "Tiêu đề bài học",
                  "type": "Topic",
                  "resource_id": 5,
                  "skills": ["Grammar"],
                  "estimated_hours": 2,
                  "is_mandatory": true,
                  "ai_reason": "Lý do AI chọn bài này"
                },
                {
                  "module_id": "QUIZ-101",
                  "title": "Quiz luyện tập: ...",
                  "type": "Quiz",
                  "resource_id": 12,
                  "skills": ["Listening"],
                  "estimated_hours": 1,
                  "is_mandatory": false,
                  "ai_reason": "Lý do AI chọn quiz này"
                }
              ]
            }
          ],
          "ai_recommendations": [
            "Khuyến nghị 1",
            "Khuyến nghị 2"
          ]
        }
        """;

    /// <summary>
    /// Builds the user-turn prompt with student profile and available resources injected.
    /// </summary>
    public static string Build(LearningPathInputDto input, int totalWeeks)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== THÔNG TIN HỌC VIÊN ===");
        sb.AppendLine($"Mục tiêu: {input.GoalName}");
        sb.AppendLine($"Trình độ hiện tại: {input.CurrentLevelName}");
        sb.AppendLine($"Trình độ mục tiêu: {input.TargetLevelName}");
        sb.AppendLine($"Thời gian học mỗi ngày: {input.AvailableMinutesPerDay} phút");
        sb.AppendLine($"Tổng thời gian: {totalWeeks} tuần");
        sb.AppendLine($"Điểm mạnh: {(string.IsNullOrWhiteSpace(input.Strengths) ? "Chưa xác định" : input.Strengths)}");
        sb.AppendLine($"Điểm yếu: {(string.IsNullOrWhiteSpace(input.Weaknesses) ? "Chưa xác định" : input.Weaknesses)}");

        if (input.SkillPriorities.Any())
        {
            sb.AppendLine($"Kỹ năng ưu tiên: {string.Join(", ", input.SkillPriorities)}");
        }

        if (input.PriorityTopics.Any())
        {
            sb.AppendLine($"Chủ đề ưu tiên: {string.Join(", ", input.PriorityTopics)}");
        }

        sb.AppendLine();
        sb.AppendLine("=== TÀI NGUYÊN HỌC TẬP KHẢ DỤNG ===");

        if (input.AvailableTopics.Any())
        {
            sb.AppendLine("Danh sách Topics (dùng type: \"Topic\", resource_id là Id):");
            foreach (var t in input.AvailableTopics)
                sb.AppendLine($"  - Id={t.Id}: {t.Name}");
        }

        if (input.AvailableLessons.Any())
        {
            sb.AppendLine("Danh sách Lessons (dùng type: \"Lesson\", resource_id là Id):");
            foreach (var l in input.AvailableLessons)
                sb.AppendLine($"  - Id={l.Id}: {l.Name}");
        }

        if (input.AvailableQuizzes.Any())
        {
            sb.AppendLine("Danh sách Quizzes (dùng type: \"Quiz\", resource_id là Id):");
            foreach (var q in input.AvailableQuizzes)
                sb.AppendLine($"  - Id={q.Id}: {q.Name}");
        }

        sb.AppendLine();
        sb.AppendLine("=== YÊU CẦU ===");
        sb.AppendLine($"Hãy tạo lộ trình {totalWeeks} tuần với 3 giai đoạn (Foundation → Practice → Mastery).");
        sb.AppendLine("Mỗi giai đoạn khoảng 4-6 modules, xen kẽ bài học và quiz.");
        sb.AppendLine("Chỉ dùng resource_id từ danh sách trên. Trả về JSON hợp lệ, không có markdown.");

        return sb.ToString();
    }
}
