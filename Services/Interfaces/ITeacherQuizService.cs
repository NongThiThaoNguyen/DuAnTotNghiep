using DuAnTotNghiep.Models.ViewModels.Teacher;

namespace DuAnTotNghiep.Services.Interfaces;

public interface ITeacherQuizService
{
    Task<List<QuizListItemViewModel>> GetQuizzesByTopicAsync(int topicId);
    Task<QuizManageViewModel?> GetQuizDetailAsync(int quizId);
    Task<int> CreateQuizAsync(CreateQuizViewModel model, int teacherId);
    Task UpdateQuizAsync(EditQuizViewModel model);
    Task DeleteQuizAsync(int quizId);
    Task<List<QuizAttemptResultViewModel>> GetQuizAttemptsAsync(int quizId);
}
