using System;
using System.Collections.Generic;

namespace StaffManagementApi.Models;

public partial class Post
{
    public int IdPost { get; set; }

    public string Name { get; set; } = null!;

    public string? Descriptions { get; set; }

    public virtual ICollection<StatusesWorker> StatusesWorkers { get; set; } = new List<StatusesWorker>();
}
