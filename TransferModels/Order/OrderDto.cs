using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferModels.Order
{
    public sealed class OrderDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PickupAt { get; set; }

        public int ClientId { get; set; }
        public int OrderTypeId { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }

        public decimal TotalPrice { get; set; }
        public int TotalCalories { get; set; }

        public int? DiscountId { get; set; }

        public List<OrderDishItemDto> Dishes { get; set; }
        public List<OrderDrinkItemDto> Drinks { get; set; }
        public List<OrderToppingDto> Toppings { get; set; }
    }
}
