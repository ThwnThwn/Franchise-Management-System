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
        public decimal? TotalAmount { get; set; }
        public CheckEnoughStockRes CheckEnoughStockRes { get; set; } = new();
    }
}
