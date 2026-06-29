using System;

namespace DuAnTotNghiep.Models
{
    public partial class ReplanningRule
    {
        public int Id { get; set; }

        public decimal LowScoreThreshold { get; set; } = 60m;

        public int MissedDaysThreshold { get; set; } = 3;

        public decimal FastProgressScoreThreshold { get; set; } = 85m;

        public bool AutoApplyEnabled { get; set; } = false;

        public int SuggestionExpiryDays { get; set; } = 7;

        public int DebounceHours { get; set; } = 24;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
