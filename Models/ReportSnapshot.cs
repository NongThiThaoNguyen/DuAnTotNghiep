using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class ReportSnapshot
{
    public int Id { get; set; }

    public string ReportType { get; set; } = null!;

    public DateOnly ReportDate { get; set; }

    public int? GeneratedBy { get; set; }

    public string ReportData { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User? GeneratedByNavigation { get; set; }
}
