using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class Role
{
    public int Id { get; set; }

    public string RoleCode { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
