using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels
{
    public class LearningPathIntegrationDto
    {
        public int StudentId { get; set; }
        public string EstimatedCefrLevel { get; set; } = null!;
        public string EstimatedLevelName { get; set; } = null!;
        public string? TargetCefrLevel { get; set; }
        public string? PrimaryGoal { get; set; }
        public int DailyStudyMinutes { get; set; }
        public int WeeklyStudyDays { get; set; }
        public List<PriorityTopicIntegrationItem> PriorityTopics { get; set; } = new();
    }

    public class PriorityTopicIntegrationItem
    {
        public int TopicId { get; set; }
        public string TopicCode { get; set; } = null!;
        public string TopicName { get; set; } = null!;
        public int SequenceOrder { get; set; }
        public string GapLevel { get; set; } = null!; // HIGH, MEDIUM, LOW
        public string RelatedSkill { get; set; } = null!;
    }
}
