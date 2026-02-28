namespace SalesService.Application.Dtos.CartItem.Response
{
    public class CartItemRep
    {
        public Guid Id { get; set; }

        public Guid? CustomerId { get; set; }

        public string Sku { get; set; } = null!;

        public string ProductName { get; set; } = null!;

        public string? Attributes { get; set; }

        public string? ImageUrl { get; set; }

        public int Quantity { get; set; }

        public decimal BasePrice { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}