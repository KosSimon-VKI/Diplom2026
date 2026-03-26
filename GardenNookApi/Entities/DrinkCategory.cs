using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class DrinkCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Drink> Drinks { get; set; } = new List<Drink>();
}
