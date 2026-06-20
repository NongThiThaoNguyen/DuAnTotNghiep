using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class SearchLog
{
    public long Id { get; set; }

    public int? UserId { get; set; }

    public string Keyword { get; set; } = null!;

    public string SearchScope { get; set; } = null!;

    public int ResultCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
