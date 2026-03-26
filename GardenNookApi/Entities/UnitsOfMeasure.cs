using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class UnitsOfMeasure
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();

    public virtual ICollection<Drink> Drinks { get; set; } = new List<Drink>();

    public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<SemiFinished> SemiFinisheds { get; set; } = new List<SemiFinished>();

    public virtual ICollection<TechnicalCardIngredientComposition> TechnicalCardIngredientCompositions { get; set; } = new List<TechnicalCardIngredientComposition>();

    public virtual ICollection<TechnicalCardSemiFinishedComposition> TechnicalCardSemiFinishedCompositions { get; set; } = new List<TechnicalCardSemiFinishedComposition>();

    public virtual ICollection<ToppingsAndSyrup> ToppingsAndSyrups { get; set; } = new List<ToppingsAndSyrup>();
}
