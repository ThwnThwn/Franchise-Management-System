using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesService.Application.Dtos.CartItem.Response
{
    public class CartItemResponse
    {
        public Guid Id { get; set; }

        public Guid? CustomerId { get; set; }

        public string Sku { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal BasePrice { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}

