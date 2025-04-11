using System;
using System.Collections.Generic;

namespace StaffManagementApi.Models;

public partial class VwWorkerDetail
{
    public int IdWorker { get; set; }

    public string? Avatar { get; set; }

    public string? FullWorkerName { get; set; }

    public DateOnly BirthDate { get; set; }

    public string? WorkEmail { get; set; }

    public string? Phone { get; set; }

    public string? PcNumber { get; set; }

    public DateOnly? StartDateStatus { get; set; }

    public DateOnly? EndDateStatus { get; set; }

    public string? Duties { get; set; }

    public int? Salary { get; set; }

    public string? Post { get; set; }

    public string? Department { get; set; }

    public string? Room { get; set; }

    public string? Workspace { get; set; }

    public string? Floor { get; set; }

    public string? Office { get; set; }

    public string? Name { get; set; }

    public DateOnly? StartDateWorkingConditions { get; set; }

    public DateOnly? EndDateWorkingConditions { get; set; }
}
