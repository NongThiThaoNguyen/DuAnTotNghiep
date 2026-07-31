using DuAnTotNghiep.Models.DTOs.PlacementTest;
using System.Text;
using System.Text.Json;

namespace DuAnTotNghiep.Services.PromptTemplates;

/// <summary>
/// Builds the Gemini prompt used to analyze a student's placement test results
/// and produce a structured CompetencyAnalysis JSON response.
/// </summary>
public static class CompetencyAnalysisPrompt
{
    /// <summary>
    /// Expected JSON schema returned by Gemini for competency analysis.
    /// </summary>
    public const string SystemPrompt = """
        Bạn là một chuyên gia phân tích năng lực tiếng Anh thông minh thuộc hệ thống "AI Study English".
        Nhiệm vụ: Phân tích kết quả bài kiểm tra đầu vào (Placement Test) của học viên và trả về bản phân tích năng lực chi tiết.

        QUAN TRỌNG: Chỉ trả về JSON hợp lệ, KHÔNG kèm markdown, KHÔNG kèm văn bản dẫn dắt.

        Cấu trúc JSON bắt buộc:
        {
          "summary": "Nhận xét tổng quan về năng lực của học viên (2-3 câu)",
          "strengths": "Danh sách điểm mạnh, phân cách bằng dấu phẩy (ví dụ: Vocabulary, Reading)",
          "weaknesses": "Danh sách điểm yếu, phân cách bằng dấu phẩy (ví dụ: Grammar, Speaking)",
          "gap_analysis": "Phân tích khoảng cách giữa trình độ hiện tại và mục tiêu (2-3 câu)",
          "confidence_score": 0.85,
          "skill_scores": [
            {
              "skill_name": "Grammar",
              "score": 0.45,
              "priority_level": 3,
              "weakness_note": "Mô tả ngắn về điểm yếu cụ thể"
            }
          ],
          "prioritized_topics": ["Topic 1", "Topic 2"],
          "knowledge_gaps": ["Gap 1", "Gap 2"]
        }
        """;

    /// <summary>
    /// Builds the full user-turn prompt with student data injected.
    /// </summary>
    public static string Build(PlacementTestAnalysisPayload payload)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== THÔNG TIN HỌC VIÊN ===");
        sb.AppendLine($"Trình độ hiện tại: {payload.CurrentLevel}");
        sb.AppendLine($"Trình độ mục tiêu: {payload.TargetLevel}");
        sb.AppendLine($"Mục tiêu học tập: {payload.LearningGoal}");
        sb.AppendLine($"Thời gian học mỗi ngày: {payload.StudyTimePerDay ?? 30} phút");
        sb.AppendLine($"Chủ đề yêu thích: {string.Join(", ", payload.PreferredTopics)}");
        sb.AppendLine();

        sb.AppendLine("=== KẾT QUẢ BÀI KIỂM TRA ===");
        sb.AppendLine($"Tổng điểm: {payload.TotalScore:F1}/100");
        sb.AppendLine($"Trình độ ước tính: {payload.EstimatedLevel}");
        sb.AppendLine();

        sb.AppendLine("=== ĐIỂM TỪNG KỸ NĂNG ===");
        foreach (var skill in payload.SkillScores)
        {
            sb.AppendLine($"- {skill.SkillName}: {skill.EarnedScore:F1}/{skill.MaxScore:F1} ({skill.Percentage:F0}%)");
        }
        sb.AppendLine();

        if (payload.TopicScores.Any())
        {
            sb.AppendLine("=== ĐIỂM TỪNG CHỦ ĐỀ ===");
            foreach (var topic in payload.TopicScores)
            {
                sb.AppendLine($"- {topic.TopicName}: {topic.Percentage:F0}%");
            }
            sb.AppendLine();
        }

        if (payload.WrongAnswers.Any())
        {
            sb.AppendLine($"=== CÁC CÂU SAI ({Math.Min(payload.WrongAnswers.Count, 10)} câu đầu) ===");
            foreach (var wrong in payload.WrongAnswers.Take(10))
            {
                sb.AppendLine($"- {JsonSerializer.Serialize(wrong)}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("=== YÊU CẦU ===");
        sb.AppendLine("Dựa vào dữ liệu trên, hãy phân tích năng lực và trả về JSON theo cấu trúc đã chỉ định.");
        sb.AppendLine("Đảm bảo skill_scores chứa tất cả kỹ năng đã có điểm, được sắp xếp theo priority_level giảm dần (điểm thấp = ưu tiên cao hơn).");

        return sb.ToString();
    }
}
