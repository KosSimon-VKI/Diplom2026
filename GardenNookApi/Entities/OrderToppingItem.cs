using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class OrderToppingItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ToppingId { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ToppingsAndSyrup Topping { get; set; } = null!;
}
