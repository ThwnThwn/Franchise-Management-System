using Ardalis.Result;
using MediatR;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler
    : IRequestHandler<UpdateOrderStatusCommand, string>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IPaymentRepository paymentRepository)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<string> Handle(UpdateOrderStatusCommand commnad,CancellationToken cancellationToken)
    {
        var request = commnad.Request;

        //Validate OrderId
        if (request.OrderId == Guid.Empty)
        {
            return "OrderId is required.";
        }

        //Load Order
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null || order.IsDeleted)
        {
            return $"Order with id '{request.OrderId}' not found.";
        }
        // Rule: Only allow Completed if payment is paid
        if (request.Status == OrderStatus.Completed)
        {
            var payments = await _paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

            bool paidExists = payments.Any(p => p.Status == (int)PaymentStatus.Paid);

            if (!paidExists)
            {
                return "Order cannot be completed because payment is not paid.";
            }
        }

        //Update Status
        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);
        return $"Order status updated successfully to '{request.Status}'.";
    }
}
