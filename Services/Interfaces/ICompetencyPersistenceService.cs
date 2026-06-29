using System.Threading.Tasks;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ICompetencyPersistenceService
    {
        // Điều phối toàn bộ luồng: nhận AI response -> validate -> map -> persist
        Task<PersistenceResultDTO> SaveAiAnalysisTransactionAsync(
            int studentId,
            int attemptId,
            AssessmentAiResponse aiResponse);
    }
}
