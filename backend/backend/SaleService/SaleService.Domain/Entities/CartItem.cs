using SalesService.Domain.Commons;
using System;

namespace SalesService.Domain.Entities
{
    public class CartItem : IEntity
    {
        public Guid Id { get; set; }
        public string Sku { get; set; }  = null!;
        public int Quantity { get; set; }
        public decimal BasePrice { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid? CustomerId { get; set; }
    }
}
