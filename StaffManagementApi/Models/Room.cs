using System;
using System.Collections.Generic;

namespace StaffManagementApi.Models;

public partial class Room
{
    public int IdRoom { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
}
