using System;

namespace DuAnTotNghiep.Models.ViewModels.Progress
{
    public class StudentTimelineItemViewModel
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string Detail { get; set; } = string.Empty;
    }
}
