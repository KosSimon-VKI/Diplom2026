using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class DishTopping
{
    public int Id { get; set; }

    public int? ToppingId { get; set; }

    public int? OrderDishItemId { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? FinalPrice { get; set; }

    public virtual OrderDishItem? OrderDishItem { get; set; }

    public virtual ToppingsAndSyrup? Topping { get; set; }
}
