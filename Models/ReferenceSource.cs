using System;
using System.Collections.Generic;
using DuAnTotNghiep.Enums;

namespace DuAnTotNghiep.Models;

public partial class ReferenceSource
{
    public int Id { get; set; }

    public string SourceName { get; set; } = null!;

    public string? SourceUrl { get; set; }

    public ReferenceSourceType SourceType { get; set; }

    public string? LicenseNote { get; set; }

    public ReferenceUsagePolicy? UsagePolicy { get; set; }

    public ReferenceReviewStatus Status { get; set; }

    public int? CreatedBy { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<TopicReference> TopicReferences { get; set; } = new List<TopicReference>();

    public virtual ICollection<ContentComplianceReview> ContentComplianceReviews { get; set; } = new List<ContentComplianceReview>();
}
