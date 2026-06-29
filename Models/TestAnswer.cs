using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class TestAnswer
{
    public int Id { get; set; }

    public int AttemptId { get; set; }

    public int QuestionId { get; set; }

    public int? SelectedOptionId { get; set; }

    public string? AnswerText { get; set; }

    public bool? IsCorrect { get; set; }

    public decimal? Score { get; set; }

    public DateTime AnsweredAt { get; set; }

    public virtual TestAttempt Attempt { get; set; } = null!;

    public virtual QuestionBank Question { get; set; } = null!;

    public virtual QuestionOption? SelectedOption { get; set; }
}
