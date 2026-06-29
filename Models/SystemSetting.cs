using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class SystemSetting
{
    public int Id { get; set; }

    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public string? Description { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
