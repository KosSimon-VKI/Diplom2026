using System.Collections.Generic;

namespace TransferModels.Orders
{
    public class OrderRequest
    {
        public int OrderTypeId { get; set; }
        public string Comment { get; set; }

        public List<OrderDishItemRequest> Dishes { get; set; }
        public List<OrderDrinkItemRequest> Drinks { get; set; } 
        public List<OrderToppingItemRequest> Toppings { get; set; }
    }

    public class OrderDishItemRequest
    {
        public int DishId { get; set; }
        public decimal Quantity { get; set; }
        public List<OrderItemToppingRequest> Toppings { get; set; } 
    }

    public class OrderDrinkItemRequest
    {
        public int DrinkId { get; set; }
        public decimal Quantity { get; set; }
        public int? MilkIngredientId { get; set; }
        public int? CoffeeIngredientId { get; set; }
        public List<OrderItemToppingRequest> Toppings { get; set; } 
    }

    // общий для топпингов у блюд/напитков
    public class OrderItemToppingRequest
    {
        public int ToppingId { get; set; }
        public decimal Quantity { get; set; } // qty "на 1 блюдо/напиток" как в твоём UI
    }

    // отдельные добавки (без привязки к блюду/напитку)
    public class OrderToppingItemRequest
    {
        public int ToppingId { get; set; }
        public int Quantity { get; set; }
    }
}
