using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class DishCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
}
