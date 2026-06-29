using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class AiGeneratedContent
{
    public int Id { get; set; }

    public int? RequestedBy { get; set; }

    public string ContentType { get; set; } = null!;

    public int? RelatedTopicId { get; set; }

    public string? PromptText { get; set; }

    public string GeneratedContent { get; set; } = null!;

    public string? AiModel { get; set; }

    public string ReviewStatus { get; set; } = null!;

    public int? ReviewedBy { get; set; }

    public string? ReviewNote { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? BatchId { get; set; }

    public int? PublishedQuestionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual LearningTopic? RelatedTopic { get; set; }

    public virtual User? RequestedByNavigation { get; set; }

    public virtual User? ReviewedByNavigation { get; set; }
}
