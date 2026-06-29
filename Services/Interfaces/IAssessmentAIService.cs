using System.Threading.Tasks;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IAssessmentAIService
    {
        Task<AssessmentAiResponse> AnalyzeAsync(AssessmentAiRequest req);
    }
}
