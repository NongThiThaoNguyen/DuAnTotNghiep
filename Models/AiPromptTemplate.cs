using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class AiPromptTemplate
{
    public int Id { get; set; }

    public string PromptCode { get; set; } = null!;

    public string PromptName { get; set; } = null!;

    public string ModuleCode { get; set; } = null!;

    public string SystemPrompt { get; set; } = null!;

    public string? UserPromptTemplate { get; set; }

    public string? OutputSchema { get; set; }

    public string Status { get; set; } = null!;

    public int VersionNo { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AiUsageLog> AiUsageLogs { get; set; } = new List<AiUsageLog>();

    public virtual User? CreatedByNavigation { get; set; }
}
