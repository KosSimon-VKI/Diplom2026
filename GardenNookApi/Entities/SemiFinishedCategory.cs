using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class SemiFinishedCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<SemiFinished> SemiFinisheds { get; set; } = new List<SemiFinished>();
}
