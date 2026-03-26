using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferModels.Order
{
    public sealed class OrderDrinkItemDto
    {
        public int DrinkId { get; set; }
        public int Quantity { get; set; }

        // DrinkToppings
        public List<OrderToppingDto> Toppings { get; set; }
    }
}
