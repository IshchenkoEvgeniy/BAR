using System;
using System.Collections.Generic;

namespace BAR.Model
{
    public class OrderHistory
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public List<CartItem> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public string UserId { get; set; }

        public OrderHistory()
        {
            OrderId = Guid.NewGuid().ToString();
            OrderDate = DateTime.Now;
            Items = new List<CartItem>();
        }
    }
}
