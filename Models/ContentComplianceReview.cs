using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class ContentComplianceReview
{
    public int Id { get; set; }

    public string ContentType { get; set; } = null!;

    public int ContentId { get; set; }

    public int ReviewerId { get; set; }

    public bool CopyrightCheck { get; set; }

    public string? PlagiarismRisk { get; set; }

    public string ReviewStatus { get; set; } = null!;

    public string? ReviewNote { get; set; }

    public DateTime ReviewedAt { get; set; }

    public int? ReferenceSourceId { get; set; }

    public virtual User Reviewer { get; set; } = null!;

    public virtual ReferenceSource? ReferenceSource { get; set; }
}
