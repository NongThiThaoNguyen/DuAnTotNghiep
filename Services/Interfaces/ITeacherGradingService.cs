using DuAnTotNghiep.Models.ViewModels.Teacher;

namespace DuAnTotNghiep.Services.Interfaces;

public interface ITeacherGradingService
{
    Task<List<PendingSubmissionViewModel>> GetPendingSubmissionsAsync(int teacherId);
    Task<PracticeSubmissionDetailViewModel?> GetSubmissionDetailAsync(int submissionId);
    Task GradeSubmissionAsync(int submissionId, decimal score, string? feedback, int teacherId);
    Task<List<GradeOverviewViewModel>> GetGradesOverviewAsync(int? topicId);
}
