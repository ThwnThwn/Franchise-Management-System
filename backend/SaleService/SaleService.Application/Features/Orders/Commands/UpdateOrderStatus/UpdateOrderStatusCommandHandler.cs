using MediatR;
using Microsoft.Extensions.Logging;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using SharedKernel.Constants;

namespace SalesService.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler
    : IRequestHandler<UpdateOrderStatusCommand, string>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IPaymentRepository paymentRepository,
        IOutboxRepository outboxRepository,
        ILogger<UpdateOrderStatusCommandHandler> logger,
        IKafkaProducer kafkaProducer)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _outboxRepository = outboxRepository;
        _logger = logger;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<string> Handle(UpdateOrderStatusCommand commnad, CancellationToken cancellationToken)
    {
        var request = commnad.Request;

        //Validate OrderIdw
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
        if (!OrderStatusTransitionPolicy.CanTransition(order.Status, request.Status))
        {
            return $"Invalid order status transition from {order.Status} to {request.Status}";
        }
        if (request.Status == OrderStatus.Completed)
        {

            if (order.PaymentMethod == PaymentMethod.VNPay)
            {
                var payment = await _paymentRepository.GetNewestByOrderIdAndStatusAsync(request.OrderId, PaymentStatus.Paid, cancellationToken);
                if (payment == null)
                {
                    return "Order cannot be completed because payment is not paid.";
                }
            }
            else
            {
                var payment = await _paymentRepository.GetNewestByOrderIdAndStatusAsync(request.OrderId, PaymentStatus.Pending, cancellationToken);
                if (payment == null)
                {
                    return "No pending payment found for this order.";
                }
                payment.Status = (int)PaymentStatus.Paid;
                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.SaveChangesAsync();
            }
        }
        order.Status = request.Status;
        // Create OrderStatusEvent
        var orderEvent = new OrderStatusEvent
        {
            OrderId = order.Id,
            OrderCode = order.Code,
            CustomerId = order.CustomerId,
            CustomerEmail = string.Empty, // Will be fetched by Consumer
            NewOrderStatus = OrderToKafka.MapOrderStatus(order.Status),
            Items = OrderToKafka.MapOrderItems(order.OrderItems),
            OrderSummary = $"Order {order.Code} created with {order.OrderItems.Count} items",
            Tax = order.Tax,
            TotalAmount = order.TotalAmount,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Serialize to JSON
        var eventJson = System.Text.Json.JsonSerializer.Serialize(orderEvent);

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "OrderStatusEvent",
            Content = eventJson,
            OccurredOn = DateTimeOffset.UtcNow,
            ProcessedOn = null,
            Error = null
        };

        // Add to Outbox Repository
        _outboxRepository.Add(outboxMessage);
        _logger.LogInformation("Added OrderCreatedEvent to Outbox for Order {OrderCode}", order.Code);

        // Publish order notification event to Kafka
        await _kafkaProducer.PublishAsync(KafkaTopics.OrderNotifications, orderEvent);
        //Update Status
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);
        return $"Order status updated successfully to '{request.Status}'";
    }

}
