using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class Order
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ClientId { get; set; }

    public int? StatusId { get; set; }

    public int? OrderTypeId { get; set; }

    public string? Comment { get; set; }

    public decimal? TotalCalories { get; set; }

    public int? DiscountId { get; set; }

    public decimal? TotalPrice { get; set; }

    public virtual Client? Client { get; set; }

    public virtual Discount? Discount { get; set; }

    public virtual OrderStatus? Status { get; set; }

    public virtual ICollection<OrderDishItem> OrderDishItems { get; set; } = new List<OrderDishItem>();

    public virtual ICollection<OrderDrinkItem> OrderDrinkItems { get; set; } = new List<OrderDrinkItem>();

    public virtual ICollection<OrderToppingItem> OrderToppingItems { get; set; } = new List<OrderToppingItem>();

    public virtual OrderType? OrderType { get; set; }
}
