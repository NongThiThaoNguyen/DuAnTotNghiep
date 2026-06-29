using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models;

public partial class PlacementTestQuestion
{
    public int Id { get; set; }

    public int SectionId { get; set; }

    public int QuestionId { get; set; }

    [Range(0, 100)]
    public decimal Points { get; set; }

    public int OrderIndex { get; set; }

    public virtual QuestionBank Question { get; set; } = null!;

    public virtual PlacementTestSection Section { get; set; } = null!;
}
