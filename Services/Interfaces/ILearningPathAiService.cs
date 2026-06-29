using DuAnTotNghiep.DTOs.LearningPath;

namespace DuAnTotNghiep.Services.Interfaces
{
    /// <summary>
    /// Generates and validates AI learning path output for module M8.
    /// </summary>
    public interface ILearningPathAiService
    {
        Task<LearningPathOutputDto> GeneratePathFromAiAsync(LearningPathInputDto input);

        Task<(bool IsValid, string[] Errors)> ValidateAiOutputAsync(
            LearningPathOutputDto output,
            LearningPathInputDto input);
    }
}
