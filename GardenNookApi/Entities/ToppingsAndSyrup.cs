using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class ToppingsAndSyrup
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal? Quantity { get; set; }

    public int? UnitOfMeasureId { get; set; }

    public decimal? CostRub { get; set; }

    public decimal? MarkupPercent { get; set; }

    public decimal? PriceRub { get; set; }

    public decimal? CostPercent { get; set; }

    public int? TechnicalCardId { get; set; }

    public decimal? FatsG { get; set; }

    public decimal? ProteinsG { get; set; }

    public decimal? CarbsG { get; set; }

    public decimal? CaloriesKcal { get; set; }

    public decimal? Kilojoules { get; set; }

    public int? CategoryId { get; set; }

    public bool IsAvailable { get; set; }

    public virtual ToppingCategory? Category { get; set; }

    public virtual ICollection<DishTopping> DishToppings { get; set; } = new List<DishTopping>();

    public virtual ICollection<DrinkTopping> DrinkToppings { get; set; } = new List<DrinkTopping>();

    public virtual ICollection<OrderToppingItem> OrderToppingItems { get; set; } = new List<OrderToppingItem>();

    public virtual TechnicalCard? TechnicalCard { get; set; }

    public virtual UnitsOfMeasure? UnitOfMeasure { get; set; }
}
