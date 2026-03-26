using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class SemiFinished
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal? CostRub { get; set; }

    public int? CategoryId { get; set; }

    public int? UnitOfMeasureId { get; set; }

    public int? TechnicalCardId { get; set; }

    public decimal? FatsG { get; set; }

    public decimal? ProteinsG { get; set; }

    public decimal? CarbsG { get; set; }

    public decimal? CaloriesKcal { get; set; }

    public decimal? Kilojoules { get; set; }

    public virtual SemiFinishedCategory? Category { get; set; }

    public virtual ICollection<Preparation> Preparations { get; set; } = new List<Preparation>();

    public virtual TechnicalCard? TechnicalCard { get; set; }

    public virtual ICollection<TechnicalCardSemiFinishedComposition> TechnicalCardSemiFinishedCompositions { get; set; } = new List<TechnicalCardSemiFinishedComposition>();

    public virtual UnitsOfMeasure? UnitOfMeasure { get; set; }
}
