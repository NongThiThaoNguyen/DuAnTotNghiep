using System;
using System.Collections.Generic;
using DuAnTotNghiep.Enums;

namespace DuAnTotNghiep.ViewModels
{
    /// <summary>
    /// ViewModel representing the historical log of replanning events for a student.
    /// </summary>
    public class ReplanningHistoryViewModel
    {
        public List<ReplanningHistoryItem> HistoryItems { get; set; } = new List<ReplanningHistoryItem>();
    }

    /// <summary>
    /// Represents an individual item in the replanning history log.
    /// </summary>
    public class ReplanningHistoryItem
    {
        public DateTime Date { get; set; }

        public TriggerType TriggerType { get; set; }

        public ReplanningStatus Status { get; set; }

        public string Summary { get; set; } = null!;

        public int? PathVersion { get; set; }
    }
}
