using System;

namespace DuAnTotNghiep.DTOs
{
    /// <summary>
    /// Data Transfer Object representing the parsed structured response returned by the AI Replanning Engine.
    /// </summary>
    public class ReplanningOutputDto
    {
        public string Summary { get; set; } = null!;

        public string TriggerReason { get; set; } = null!;

        public NodeChangeDto[] Changes { get; set; } = Array.Empty<NodeChangeDto>();

        public string StudentMessage { get; set; } = null!;

        public string RiskLevel { get; set; } = null!;
    }

    /// <summary>
    /// Represents a proposed path node adjustment returned by the AI.
    /// </summary>
    public class NodeChangeDto
    {
        public string NodeName { get; set; } = null!;

        public string ChangeType { get; set; } = null!; // E.g., ADDED, REMOVED, DATE_CHANGED

        public DateTime? OldDate { get; set; }

        public DateTime? NewDate { get; set; }

        public string ReviewReason { get; set; } = null!;
    }
}
