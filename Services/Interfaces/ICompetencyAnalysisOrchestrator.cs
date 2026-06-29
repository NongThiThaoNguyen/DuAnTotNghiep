using System.Threading.Tasks;
using DuAnTotNghiep.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ICompetencyAnalysisOrchestrator
    {
        /// <summary>
        /// Retrieves the latest competency analysis for a student.
        /// </summary>
        Task<CompetencyResultViewModel?> GetLatestAnalysisAsync(int studentId);

        /// <summary>
        /// Retrieves a specific analysis by ID, with ownership validation.
        /// </summary>
        Task<CompetencyResultViewModel?> GetAnalysisByIdAsync(int analysisId, int currentUserId, string role, string? ipAddress = null);

        /// <summary>
        /// Triggers a regeneration of the analysis using AI.
        /// </summary>
        Task<bool> TriggerRegenerationAsync(int analysisId, int studentId);

        /// <summary>
        /// Retrieves the standardized integration DTO for Module M8 AI Learning Path generation.
        /// </summary>
        Task<LearningPathIntegrationDto?> GetLearningPathInputAsync(int analysisId, int currentUserId, string role);
    }
}
