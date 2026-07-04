using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces;

public interface IAdminQuizManagementService
{
    Task<QuizManagementIndexViewModel> GetAllQuizzesAsync(int? topicId = null, int? teacherId = null, int page = 1, int pageSize = 20);
    Task<QuizManagementDetailsViewModel?> GetQuizDetailsAsync(int quizId);
    Task<QuizAttemptsAdminViewModel?> GetQuizAttemptsAsync(int quizId);
}
