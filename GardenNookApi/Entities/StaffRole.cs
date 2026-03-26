using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class StaffRole
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
