using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class OrderDrinkItemModifier
{
    public int Id { get; set; }

    public int OrderDrinkItemId { get; set; }

    public int? MilkIngredientId { get; set; }

    public int? CoffeeIngredientId { get; set; }

    public virtual Ingredient? CoffeeIngredient { get; set; }

    public virtual Ingredient? MilkIngredient { get; set; }

    public virtual OrderDrinkItem OrderDrinkItem { get; set; } = null!;
}
