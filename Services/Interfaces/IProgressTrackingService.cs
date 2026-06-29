using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IProgressTrackingService
    {
        Task<bool> RecordLessonCompleted(int studentId, int lessonId, int? durationMinutes = null);
        Task<bool> RecordQuizSubmitted(int studentId, int quizId, decimal score, int? durationMinutes = null);
        Task<bool> RecordFeedbackViewed(int studentId, int feedbackId, bool triggerReview = false);
        Task<bool> RecordTutorMessage(int studentId, int tutorSessionId, int? durationMinutes = null);
        Task<bool> UpdateNodeProgress(int studentId, int learningPathNodeId);
        Task<bool> RecalculateStudentProgress(int studentId);
    }
}
