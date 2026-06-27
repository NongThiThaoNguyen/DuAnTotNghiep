using System;

namespace DuAnTotNghiep.DTOs.Progress
{
    public class HistoryFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? ActivityType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
