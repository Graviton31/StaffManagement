using System;
using System.Collections.Generic;

namespace StaffManagementApi.Models;

public partial class Workspace
{
    public int IdWorkspace { get; set; }

    public string Name { get; set; } = null!;

    public int IdWorker { get; set; }

    public int IdOffice { get; set; }

    public int IdFloor { get; set; }

    public int IdRoom { get; set; }

    public virtual Floor IdFloorNavigation { get; set; } = null!;

    public virtual Office IdOfficeNavigation { get; set; } = null!;

    public virtual Room IdRoomNavigation { get; set; } = null!;

    public virtual Worker IdWorkerNavigation { get; set; } = null!;
}
