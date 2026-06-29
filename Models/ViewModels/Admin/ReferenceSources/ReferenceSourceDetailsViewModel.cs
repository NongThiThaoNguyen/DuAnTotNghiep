using DuAnTotNghiep.Models.Enums;
using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.Admin.ReferenceSources
{
    public class ReferenceSourceDetailsViewModel
    {
        // General Info
        public int Id { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string? SourceUrl { get; set; }
        public ReferenceSourceType SourceType { get; set; }
        public string? Author { get; set; }
        public string? Organization { get; set; }
        public string? Description { get; set; }

        // Compliance Info
        public string? LicenseNote { get; set; }
        public ReferenceUsagePolicy? UsagePolicy { get; set; }

        // Workflow Status
        public ReferenceReviewStatus Status { get; set; }
        public int? CreatedById { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? ApprovedById { get; set; }
        public string ApprovedByUserName { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public bool IsActive { get; set; }

        // Linked References
        public List<TopicReferenceDto> LinkedTopics { get; set; } = new List<TopicReferenceDto>();
        public List<LessonReferenceDto> LinkedLessons { get; set; } = new List<LessonReferenceDto>();

        // Auditing
        public List<AuditLogDto> RecentAuditLogs { get; set; } = new List<AuditLogDto>();

        // Flags for dynamic actions
        public bool CanEdit { get; set; }
        public bool CanReview { get; set; }
        public bool CanArchive { get; set; }
        public bool CanSubmit { get; set; }
    }

    public class TopicReferenceDto
    {
        public int Id { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? ReferenceNote { get; set; }
    }

    public class LessonReferenceDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TopicTitle { get; set; } = string.Empty;
    }

    public class AuditLogDto
    {
        public long Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ActorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }
}
