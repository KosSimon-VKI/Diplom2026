using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class Drink
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal? Quantity { get; set; }

    public int? UnitOfMeasureId { get; set; }

    public int? CategoryId { get; set; }

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

    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; }

    public virtual DrinkCategory? Category { get; set; }

    public virtual ICollection<OrderDrinkItem> OrderDrinkItems { get; set; } = new List<OrderDrinkItem>();

    public virtual TechnicalCard? TechnicalCard { get; set; }

    public virtual UnitsOfMeasure? UnitOfMeasure { get; set; }
}
