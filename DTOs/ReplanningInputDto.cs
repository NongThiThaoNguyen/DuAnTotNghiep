using System;
using DuAnTotNghiep.Enums;

namespace DuAnTotNghiep.DTOs
{
    /// <summary>
    /// Data Transfer Object representing the input data package sent to the AI Replanning Engine.
    /// </summary>
    public class ReplanningInputDto
    {
        public int StudentId { get; set; }

        public string[] WeakTopics { get; set; } = Array.Empty<string>();

        public int MissedTaskCount { get; set; }

        public decimal AverageScore { get; set; }

        public TriggerType TriggerType { get; set; }

        public string CurrentPathSummary { get; set; } = null!;

        public string RecentActivitySummary { get; set; } = null!;
    }
}
