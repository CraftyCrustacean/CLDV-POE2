using C2B_Functions.Models;
using System.Collections.Generic;

namespace C2B_Functions.Models
{
    public class OrderMessage
    {
        public Order Order { get; set; } = new Order();
        public List<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}
