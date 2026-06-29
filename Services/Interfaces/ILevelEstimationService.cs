using System.Threading.Tasks;
using DuAnTotNghiep.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ILevelEstimationService
    {
        // Ước lượng trình độ hiện tại và so sánh mục tiêu
        Task<LevelEstimationResultDTO> EstimateStudentLevelAsync(int studentId, int attemptId, decimal aiSuggestedScore);
        
        // Phân tích khoảng cách (Gap) phục vụ giao diện
        Task<LevelGapViewModel> GetLevelGapAnalysisAsync(int analysisId, int currentUserId);
    }
}
