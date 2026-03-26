using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class Inventory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal? Stock { get; set; }

    public int? UnitOfMeasureId { get; set; }

    public decimal? CostRub { get; set; }

    public int? CategoryId { get; set; }

    public virtual UnitsOfMeasure? UnitOfMeasure { get; set; }
}
