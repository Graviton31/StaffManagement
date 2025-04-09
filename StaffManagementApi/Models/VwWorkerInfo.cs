using System;
using System.Collections.Generic;

namespace StaffManagementApi.Models;

public partial class VwWorkerInfo
{
    public int IdWorker { get; set; }

    public string? Avatar { get; set; }

    public string? FullWorkerName { get; set; }

    public string? Post { get; set; }

    public string? Department { get; set; }

    public DateOnly BirthDate { get; set; }
}
