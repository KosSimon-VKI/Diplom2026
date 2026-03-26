using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class OrderDishItem
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? DishId { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? FinalPrice { get; set; }

    public virtual Dish? Dish { get; set; }

    public virtual ICollection<DishTopping> DishToppings { get; set; } = new List<DishTopping>();

    public virtual Order? Order { get; set; }
}
