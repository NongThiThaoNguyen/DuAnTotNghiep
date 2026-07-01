using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DuAnTotNghiep.Models.ViewModels.Teacher;

public class QuizIndexViewModel
{
    public int TopicId { get; set; }
    public string TopicTitle { get; set; } = string.Empty;
    public List<QuizListItemViewModel> Quizzes { get; set; } = new();
}

public class QuizListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int? TopicId { get; set; }
    public int QuestionCount { get; set; }
    public int AttemptCount { get; set; }
    public decimal AverageScore { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public decimal? PassScore { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class QuizManageViewModel
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public decimal? PassScore { get; set; }
    public string Status { get; set; } = string.Empty;
    [ValidateNever]
    public List<QuizQuestionFormViewModel> Questions { get; set; } = new();
}

public class CreateQuizViewModel
{
    public int TopicId { get; set; }
    public string TopicTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TimeLimitMinutes { get; set; } = 15;
    public decimal? PassScore { get; set; } = 50;
    [ValidateNever]
    public List<QuizQuestionFormViewModel> Questions { get; set; } = new();
}

public class EditQuizViewModel : CreateQuizViewModel
{
    public int Id { get; set; }
    public string Status { get; set; } = "PUBLISHED";
}

public class QuizQuestionFormViewModel
{
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "MCQ";
    [ValidateNever]
    public List<string> Options { get; set; } = new();
    public string? CorrectAnswer { get; set; }
    public decimal Points { get; set; } = 1;
}

public class QuizAttemptResultViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public TimeSpan? TimeTaken { get; set; }
}
