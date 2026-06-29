<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
=======
using System;
using System.Collections.Generic;
using DuAnTotNghiep.Enums;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52

namespace DuAnTotNghiep.Models;

public partial class ReferenceSource
{
    public int Id { get; set; }

    public string SourceName { get; set; } = null!;

    public string? SourceUrl { get; set; }

<<<<<<< HEAD
    public string SourceType { get; set; } = null!;

    public string? LicenseNote { get; set; }

    public string? UsagePolicy { get; set; }

    public string Status { get; set; } = null!;
=======
    public ReferenceSourceType SourceType { get; set; }

    public string? LicenseNote { get; set; }
    
    public string? ComplianceEvidenceUrl { get; set; }

    public string? Author { get; set; }

    public string? Organization { get; set; }

    public string? Description { get; set; }

    public ReferenceUsagePolicy? UsagePolicy { get; set; }

    public ReferenceReviewStatus Status { get; set; }
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52

    public int? CreatedBy { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

<<<<<<< HEAD
    public DateTime CreatedAt { get; set; }

=======
    public int? RejectedBy { get; set; }

    public DateTime? RejectedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

<<<<<<< HEAD
    public virtual ICollection<TopicReference> TopicReferences { get; set; } = new List<TopicReference>();
=======
    public virtual User? RejectedByNavigation { get; set; }

    public virtual ICollection<TopicReference> TopicReferences { get; set; } = new List<TopicReference>();

    public virtual ICollection<ContentComplianceReview> ContentComplianceReviews { get; set; } = new List<ContentComplianceReview>();
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
}
