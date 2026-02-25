using MediatR;
using SalesService.Application.Dtos.Inventory;

namespace SalesService.Application.Features.Orders.Commands.Update
{
    public record class UpdateOrderCommand(UpdateOrderReq Request) : IRequest<UpdateOrderRes>;

    public sealed class UpdateOrderReq
    {
        public Guid OrderId { get; set; }
        public List<OrderItem> Items { get; set; } = new();

        public Guid UserId { get; set; } = default!;
        public string RoleSlug { get; set; } = default!;
    }

    public sealed class OrderItem
    {
        public string Sku { get; set; } = default!;
        public int Quantity { get; set; }

    }   

    public sealed class UpdateOrderRes
    {
        public Guid? OrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public string? OrderCode { get; set; } = default!;
        public List<OrderItemDto> Items { get; set; } = new();
        public decimal? TotalAmount { get; set; }
        public CheckEnoughStockRes CheckEnoughStockRes { get; set; } = new();
    }
    public sealed class OrderItemDto
    {
        public string Sku { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

    }
}
