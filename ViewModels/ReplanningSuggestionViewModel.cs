using System;
using System.Collections.Generic;
using DuAnTotNghiep.Enums;

namespace DuAnTotNghiep.ViewModels
{
    /// <summary>
    /// ViewModel representing a proposed learning path replan suggestion shown to the student.
    /// This intentionally excludes developer metrics like prompt, tokens, and model details.
    /// </summary>
    public class ReplanningSuggestionViewModel
    {
        public int EventId { get; set; }

        public string TriggerReason { get; set; } = null!;

        public string StudentMessage { get; set; } = null!;

        public List<NodeChangeItem> Changes { get; set; } = new List<NodeChangeItem>();

        public ReplanningStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public string RiskLevel { get; set; } = null!;
    }

    /// <summary>
    /// Represents a specific learning path node adjustment.
    /// </summary>
    public class NodeChangeItem
    {
        public string NodeName { get; set; } = null!;

        public string ChangeType { get; set; } = null!; // E.g., ADDED, REMOVED, DATE_CHANGED, COMPLETED

        public DateTime? OldDate { get; set; }

        public DateTime? NewDate { get; set; }

        public string ReviewReason { get; set; } = null!;
    }
}
