using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class Ingredient
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal? Stock { get; set; }

    public int? UnitOfMeasureId { get; set; }

    public decimal? CostRub { get; set; }

    public int? CategoryId { get; set; }

    public virtual IngredientCategory? Category { get; set; }

    public virtual ICollection<OrderDrinkItemModifier> OrderDrinkItemModifiersCoffeeIngredients { get; set; } = new List<OrderDrinkItemModifier>();

    public virtual ICollection<OrderDrinkItemModifier> OrderDrinkItemModifiersMilkIngredients { get; set; } = new List<OrderDrinkItemModifier>();

    public virtual ICollection<TechnicalCardIngredientComposition> TechnicalCardIngredientCompositions { get; set; } = new List<TechnicalCardIngredientComposition>();

    public virtual UnitsOfMeasure? UnitOfMeasure { get; set; }
}
