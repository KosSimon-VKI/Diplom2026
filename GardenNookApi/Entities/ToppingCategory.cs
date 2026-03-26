using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class ToppingCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<ToppingsAndSyrup> ToppingsAndSyrups { get; set; } = new List<ToppingsAndSyrup>();
}
