<<<<<<< HEAD
using System;
using DuAnTotNghiep.Enums;

namespace DuAnTotNghiep.Models
{
    public partial class AiReplanningEvent
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int LearningPathId { get; set; }

        public TriggerType TriggerType { get; set; }

        public string? OldPlanSummary { get; set; }

        public string? NewPlanSummary { get; set; }

        public string Reason { get; set; } = null!;

        public ReplanningStatus Status { get; set; }

        public string? ChangedNodesJson { get; set; }

        public bool? AcceptedByUser { get; set; }

        public DateTime? AcceptedAt { get; set; }

        public DateTime? AppliedAt { get; set; }

        public int? PathVersionNo { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual StudentLearningPath LearningPath { get; set; } = null!;

        public virtual User Student { get; set; } = null!;
    }
=======
﻿using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class AiReplanningEvent
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int LearningPathId { get; set; }

    public string TriggerType { get; set; } = null!;

    public string? OldPlanSummary { get; set; }

    public string? NewPlanSummary { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual StudentLearningPath LearningPath { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
}
