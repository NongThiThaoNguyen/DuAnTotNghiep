using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class ReferenceSource
{
    public int Id { get; set; }

    public string SourceName { get; set; } = null!;

    public string? SourceUrl { get; set; }

    public string SourceType { get; set; } = null!;

    public string? LicenseNote { get; set; }

    public string? UsagePolicy { get; set; }

    public string Status { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<TopicReference> TopicReferences { get; set; } = new List<TopicReference>();
}
