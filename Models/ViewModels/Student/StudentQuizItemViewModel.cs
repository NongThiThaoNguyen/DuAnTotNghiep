using System;

namespace DuAnTotNghiep.Models.ViewModels.Student
{
    public class StudentQuizItemViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int TimeLimit { get; set; }
        
        public bool IsCompleted { get; set; }
        public decimal HighestScore { get; set; }
        public DateTime? LastAttemptDate { get; set; }
        public int TopicId { get; set; } // Needed for routing to Take(topicId)
    }
}
