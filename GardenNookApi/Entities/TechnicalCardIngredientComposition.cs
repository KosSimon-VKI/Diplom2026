using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class TechnicalCardIngredientComposition
{
    public int Id { get; set; }

    public int? TechnicalCardId { get; set; }

    public int? IngredientId { get; set; }

    public int? UnitOfMeasureId { get; set; }

    public decimal? GrossWeight { get; set; }

    public decimal? ColdLossPercent { get; set; }

    public decimal? NetWeight { get; set; }

    public decimal? HotLossPercent { get; set; }

    public decimal? OutputWeight { get; set; }

    public virtual Ingredient? Ingredient { get; set; }

    public virtual TechnicalCard? TechnicalCard { get; set; }

    public virtual UnitsOfMeasure? UnitOfMeasure { get; set; }
}
