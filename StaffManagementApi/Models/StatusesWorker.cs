using System;
using System.Collections.Generic;

namespace StaffManagementApi.Models;

public partial class StatusesWorker
{
    public int IdStatusWorker { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Duties { get; set; }

    public int? Salary { get; set; }

    public int IdWorker { get; set; }

    public int IdPost { get; set; }

    public int IdDepartment { get; set; }

    public virtual Department IdDepartmentNavigation { get; set; } = null!;

    public virtual Post IdPostNavigation { get; set; } = null!;

    public virtual Worker IdWorkerNavigation { get; set; } = null!;
}
