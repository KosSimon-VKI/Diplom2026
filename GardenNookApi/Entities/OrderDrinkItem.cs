using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class OrderDrinkItem
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? DrinkId { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? FinalPrice { get; set; }

    public virtual Drink? Drink { get; set; }

    public virtual ICollection<DrinkTopping> DrinkToppings { get; set; } = new List<DrinkTopping>();

    public virtual OrderDrinkItemModifier? OrderDrinkItemModifier { get; set; }

    public virtual Order? Order { get; set; }
}
