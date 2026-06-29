using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.ViewModels
{
    public class AiContentReviewDetailViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Content Type")]
        public string? ContentType { get; set; }

        [Display(Name = "Question Text")]
        public string? QuestionText { get; set; }

        [Display(Name = "Options (JSON)")]
        public string? Options { get; set; }

        [Display(Name = "Correct Answer Index")]
        public int? CorrectAnswerIndex { get; set; }

        [Display(Name = "Explanation")]
        [StringLength(2000)]
        public string? Explanation { get; set; }

        [Display(Name = "Difficulty")]
        public string? Difficulty { get; set; }

        [Display(Name = "Skill Tags")]
        public string? SkillTags { get; set; }

        [Display(Name = "Full Generated Content (JSON)")]
        public string? GeneratedContent { get; set; }

        [Display(Name = "Original Prompt")]
        public string? PromptText { get; set; }

        public string? ReviewStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? RequestedBy { get; set; }
        public string? RequestedByName { get; set; }
        public int? RelatedTopicId { get; set; }
        public string? TopicName { get; set; }

        // Review form fields
        [Display(Name = "Copyright Check")]
        public bool? CopyrightCheck { get; set; }

        [Display(Name = "Plagiarism Risk")]
        [StringLength(50)]
        public string? PlagiarismRisk { get; set; } // LOW, MEDIUM, HIGH

        [Display(Name = "Review Notes")]
        [StringLength(2000)]
        public string? ReviewNote { get; set; }

        [Display(Name = "Action")]
        public string? ReviewAction { get; set; } // APPROVE, REJECT, REVISION

        // Flag for UI: show publish button only if APPROVED
        public bool IsApproved => ReviewStatus == "APPROVED";

        // Published reference
        public int? PublishedQuestionId { get; set; }
    }
}
