using System;
using System.Collections.Generic;

namespace StaffManagementApi.Models;

public partial class WorkingCondition
{
    public int IdWorkCondition { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Description { get; set; }

    public int IdWorker { get; set; }

    public virtual Worker IdWorkerNavigation { get; set; } = null!;
}
