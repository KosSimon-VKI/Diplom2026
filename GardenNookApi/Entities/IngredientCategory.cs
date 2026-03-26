using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class IngredientCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}
