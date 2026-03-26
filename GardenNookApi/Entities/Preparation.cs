using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class Preparation
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? SemiFinishedId { get; set; }

    public decimal? StockGrams { get; set; }

    public DateOnly? ProductionDate { get; set; }

    public virtual SemiFinished? SemiFinished { get; set; }
}
