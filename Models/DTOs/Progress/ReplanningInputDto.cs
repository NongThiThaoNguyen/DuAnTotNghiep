using System.Collections.Generic;

namespace DuAnTotNghiep.Models.DTOs.Progress
{
    public class ReplanningInputDto
    {
        public int StudentId { get; set; }
        public int PathId { get; set; }
        public List<int> RemainingNodes { get; set; } = new();
        public List<string> WeakTopics { get; set; } = new();
        public List<string> InactiveDays { get; set; } = new();
        public List<string> FastImprovementTopics { get; set; } = new();
    }
}
