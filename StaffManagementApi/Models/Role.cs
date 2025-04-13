using System;
using System.Collections.Generic;

namespace StaffManagementApi.Models;

public partial class Role
{
    public int IdRole { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
}
