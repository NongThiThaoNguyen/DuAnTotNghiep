using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models;

public partial class PlacementTestSection
{
    public int Id { get; set; }

    public int PlacementTestId { get; set; }

    public int SkillId { get; set; }

    [Required]
    [MaxLength(255)]
    public string SectionName { get; set; } = null!;

    public string? Instruction { get; set; }

    public int OrderIndex { get; set; }

    [Range(0, 999.99)]
    public decimal MaxScore { get; set; }

    public virtual PlacementTest PlacementTest { get; set; } = null!;

    public virtual ICollection<PlacementTestQuestion> PlacementTestQuestions { get; set; } = new List<PlacementTestQuestion>();

    public virtual EnglishSkill Skill { get; set; } = null!;
}
