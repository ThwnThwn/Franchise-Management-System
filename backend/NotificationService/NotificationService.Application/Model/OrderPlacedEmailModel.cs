using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Application.Model
{
    public sealed class OrderPlacedEmailModel
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = default!;
        public decimal TotalAmount { get; set; }

        public decimal Tax { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
    public sealed class CartItemDto
    {
        public string Sku { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
