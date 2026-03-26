using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class DrinkTopping
{
    public int Id { get; set; }

    public int? ToppingId { get; set; }

    public int? OrderDrinkItemId { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? FinalPrice { get; set; }

    public virtual OrderDrinkItem? OrderDrinkItem { get; set; }

    public virtual ToppingsAndSyrup? Topping { get; set; }
}
