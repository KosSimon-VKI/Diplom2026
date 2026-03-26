using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferModels.Order
{
    public sealed class OrderToppingDto
    {
        public int ToppingId { get; set; }
        public int Quantity { get; set; }
    }
}
