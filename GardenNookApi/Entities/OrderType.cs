using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class OrderType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
