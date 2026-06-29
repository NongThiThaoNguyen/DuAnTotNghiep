using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class PracticeSubmission
{
    public int Id { get; set; }

    public int PracticeTaskId { get; set; }

    public int StudentId { get; set; }

    public string? SubmissionText { get; set; }

    public string? FileUrl { get; set; }

    public string? AudioUrl { get; set; }

    public DateTime SubmittedAt { get; set; }

    public decimal? Score { get; set; }

    public string? AiFeedback { get; set; }

    public string? TeacherFeedback { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<AiFeedback> AiFeedbacks { get; set; } = new List<AiFeedback>();

    public virtual PracticeTask PracticeTask { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
