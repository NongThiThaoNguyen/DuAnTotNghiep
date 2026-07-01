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
    Task AddQuestionAsync(int quizId, QuizQuestionFormViewModel question, int teacherId);
    Task DeleteQuestionAsync(int quizId, int questionId);
    Task AddQuestionsFromAiAsync(int quizId, List<DuAnTotNghiep.Models.ViewModels.GeneratedQuestionPreviewViewModel> items, int teacherId);
    Task AddQuestionsFromFileAsync(int quizId, System.IO.Stream fileStream, string fileName, int teacherId);
}
