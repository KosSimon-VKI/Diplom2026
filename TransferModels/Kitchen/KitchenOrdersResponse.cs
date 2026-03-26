using System;
using System.Collections.Generic;

namespace TransferModels.Kitchen
{
    public class KitchenOrdersResponse
    {
        public List<KitchenOrderDto> Orders { get; set; } = new List<KitchenOrderDto>();
    }

    public class KitchenOrderDto
    {
        public int OrderId { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? PickupAt { get; set; }
        public string OrderType { get; set; } = string.Empty;
        public List<KitchenOrderDishDto> Dishes { get; set; } = new List<KitchenOrderDishDto>();
        public List<KitchenOrderStandaloneToppingDto> Toppings { get; set; } = new List<KitchenOrderStandaloneToppingDto>();
    }

    public class KitchenOrderDishDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public List<KitchenOrderDishToppingDto> Toppings { get; set; } = new List<KitchenOrderDishToppingDto>();
    }

    public class KitchenOrderDishToppingDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }

    public class KitchenOrderStandaloneToppingDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }
}
