using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models.ViewModels.AILearn;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IStudentQuizService
    {
        Task<QuizViewModel?> GetQuizForTakingAsync(int topicId);
        Task<QuizViewModel?> GetQuizByIdAsync(int quizId);
        Task<QuizResultDto> SubmitQuizAsync(int quizId, int userId, Dictionary<int, string> answers);
        Task<List<DuAnTotNghiep.Models.ViewModels.Student.StudentQuizItemViewModel>> GetAllQuizzesAsync(int userId);
    }

    public class QuizResultDto
    {
        public decimal Score { get; set; }
        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
    }
}
