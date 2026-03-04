using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Orders.Queries.GetById
{
    public sealed record GetOrderByIdQuery(Guid OrderId): IRequest<Result<GetOrderByIdResponse>>;

    public sealed class GetOrderByIdResponse
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = default!;
        public decimal Subtotal { get; set; }
        public decimal? Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public int? PaymentMethod { get; set; }
        public int Status { get; set; }
        public DateTimeOffset PaidAt { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? StoreId { get; set; }
        public OrderType? Type { get; set; }

        public List<OrderItemDto> Items { get; set; } = new();
    }

    public sealed class OrderItemDto
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ProductName { get; set; } = default!;
        public string Sku { get; set; } = default!;
        public string? ImageUrl { get; set; }
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
    }
}