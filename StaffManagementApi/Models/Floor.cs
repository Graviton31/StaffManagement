using System;
using System.Collections.Generic;

namespace StaffManagementApi.Models;

public partial class Floor
{
    public int IdFloor { get; set; }

    public string Namber { get; set; } = null!;

    public virtual ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
}
