<<<<<<< HEAD
﻿using System;
=======
using System;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class AiUsageLog
{
    public long Id { get; set; }

    public int? UserId { get; set; }

    public string ModuleCode { get; set; } = null!;

    public int? PromptTemplateId { get; set; }

    public string? AiModel { get; set; }

    public int? InputTokens { get; set; }

    public int? OutputTokens { get; set; }

    public decimal? CostEstimate { get; set; }

    public string RequestStatus { get; set; } = null!;

    public string? ErrorMessage { get; set; }

<<<<<<< HEAD
    public int? DurationMs { get; set; }
=======
    public string? PromptInput { get; set; }

    public string? ResponseOutput { get; set; }

    public int? LatencyMs { get; set; }
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52

    public DateTime CreatedAt { get; set; }

    public virtual AiPromptTemplate? PromptTemplate { get; set; }

    public virtual User? User { get; set; }
}
