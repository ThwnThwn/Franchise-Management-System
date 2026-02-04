using MediatR;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Orders.Commands.UpdateOrderStatus
{
    public record UpdateOrderStatusCommand(UpdateOrderStatusReq Request) : IRequest<string>;

    public sealed class UpdateOrderStatusReq
    {
        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
    }
}
