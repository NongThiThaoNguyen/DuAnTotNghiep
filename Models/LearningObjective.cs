using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuAnTotNghiep.Models;

public partial class LearningObjective
{
    public int Id { get; set; }

    [ForeignKey("Topic")]
    public int TopicId { get; set; }

    [Required]
    public string ObjectiveText { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string CognitiveLevel { get; set; } = null!;

    public int OrderIndex { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual LearningTopic Topic { get; set; } = null!;
}
