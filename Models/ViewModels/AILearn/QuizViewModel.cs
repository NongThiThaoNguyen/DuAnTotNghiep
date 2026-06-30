using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.AILearn;

public class QuizViewModel
{
    public int QuizId { get; set; }
    public string CourseTitle { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int TimeLimitMinutes { get; set; }
    public int TotalQuestions => Questions.Count;

    public List<QuizQuestionViewModel> Questions { get; set; } = new();
}

public class QuizQuestionViewModel
{
    public int Id { get; set; } // QuestionBank Id
    public string QuestionText { get; set; } = "";
    public string Explanation { get; set; } = "";
    public string CorrectAnswer { get; set; } = ""; // A, B, C, or D or choice text
    public List<QuizOptionViewModel> Options { get; set; } = new();
}

public class QuizOptionViewModel
{
    public int Id { get; set; } // Option Id
    public string OptionText { get; set; } = "";
    public bool IsCorrect { get; set; }
}
