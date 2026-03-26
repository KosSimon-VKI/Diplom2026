using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferModels.Order
{
    public sealed class OrderDishItemDto
    {
        public int DishId { get; set; }
        public int Quantity { get; set; }

        // DishToppings
        public List<OrderToppingDto> Toppings { get; set; }
    }
}
