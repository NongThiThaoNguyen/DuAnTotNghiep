using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class QuizAttempt
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public int StudentId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public decimal? Score { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<AiFeedback> AiFeedbacks { get; set; } = new List<AiFeedback>();

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();

    public virtual User Student { get; set; } = null!;
}
