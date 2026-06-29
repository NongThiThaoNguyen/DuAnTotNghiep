using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ICompetencyFeedbackService
    {
        // Parse dữ liệu JSON đã lưu trong DB → DTO có cấu trúc 4 section để Razor View render
        Task<CompetencyFeedbackDTO> ParseAndFormatAiFeedbackAsync(int analysisId, int currentUserId);

        // Tạo danh sách hành động gợi ý từ weaknesses + gap_analysis đã lưu
        Task<List<RecommendedActionDTO>> GenerateRecommendedActionsAsync(int analysisId);
    }
}
