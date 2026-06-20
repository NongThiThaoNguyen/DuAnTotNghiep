using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class QuizQuestion
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public int QuestionId { get; set; }

    public decimal Points { get; set; }

    public int OrderIndex { get; set; }

    public virtual QuestionBank Question { get; set; } = null!;

    public virtual Quiz Quiz { get; set; } = null!;
}
