using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class Staff
{
    public int Id { get; set; }

    public int? RoleId { get; set; }

    public string? FullName { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public virtual StaffRole? Role { get; set; }
}
