using System;
using System.Collections.Generic;

namespace StaffManagementApi.Models;

public partial class Worker
{
    public int IdWorker { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string? Avatar { get; set; }

    public DateOnly BirthDate { get; set; }

    public string? WorkEmail { get; set; }

    public string? Phone { get; set; }

    public string? PcNumber { get; set; }

    public sbyte IsDeleted { get; set; }

    public int IdRole { get; set; }

    public virtual Role IdRoleNavigation { get; set; } = null!;

    public virtual ICollection<StatusesWorker> StatusesWorkers { get; set; } = new List<StatusesWorker>();

    public virtual ICollection<WorkingCondition> WorkingConditions { get; set; } = new List<WorkingCondition>();

    public virtual ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
}
