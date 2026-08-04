using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.Student
{
    /// <summary>
    /// ViewModel for the student Topic detail page, showing topic info,
    /// learning objectives, and child nodes (lessons, quizzes, etc.)
    /// for the current student's learning path.
    /// </summary>
    public class TopicDetailViewModel
    {
        // ── Topic metadata ──────────────────────────────────────────
        public int TopicId { get; set; }
        public string TopicTitle { get; set; } = string.Empty;
        public string? TopicDescription { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string SkillCode { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public int? EstimatedMinutes { get; set; }
        public string? ThumbnailUrl { get; set; }

        // ── The specific learning path node that was opened ─────────
        public int NodeId { get; set; }
        public string NodeStatus { get; set; } = string.Empty;
        public string? AiReason { get; set; }
        public string? PathPhase { get; set; }

        // ── Learning objectives for this topic ──────────────────────
        public List<TopicObjectiveItem> Objectives { get; set; } = new();

        // ── Child nodes in the student's path under this topic ──────
        public List<TopicChildNodeItem> ChildNodes { get; set; } = new();

        // ── Navigation ──────────────────────────────────────────────
        public int? PreviousNodeId { get; set; }
        public int? NextNodeId { get; set; }
    }

    public class TopicObjectiveItem
    {
        public string ObjectiveText { get; set; } = string.Empty;
        public string CognitiveLevel { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }

    public class TopicChildNodeItem
    {
        public int NodeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string NodeType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? EstimatedMinutes { get; set; }
        public System.DateOnly? ScheduledDate { get; set; }
        public bool IsClickable { get; set; }
        public string TargetUrl { get; set; } = string.Empty;
    }
}
