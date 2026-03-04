using MediatR;
using Microsoft.Extensions.Logging;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Extensions;
using SharedKernel.Infrastructure.Kafka;

namespace SalesService.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler
    : IRequestHandler<UpdateOrderStatusCommand, string>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;
    private readonly IInventoryServiceClient _inventoryServiceClient;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IPaymentRepository paymentRepository,
        IOutboxRepository outboxRepository,
        ILogger<UpdateOrderStatusCommandHandler> logger,
        IKafkaProducer kafkaProducer, IInventoryServiceClient inventoryServiceClient)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _outboxRepository = outboxRepository;
        _logger = logger;
        _kafkaProducer = kafkaProducer;
        _inventoryServiceClient = inventoryServiceClient;
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

        // Đơn hàng hủy thì xóa queue 
        if(request.Status == OrderStatus.Cancelled)
        {
            await _inventoryServiceClient.ReleaseStockAsync(new ReleaseStockReq
            {
                OrderId = order.Id,
                StoreId = order.StoreId!.Value
            });
        }

        // Đơn hàng hoàn thành
        if (request.Status == OrderStatus.Completed)
        {

            if (order.PaymentMethod == PaymentMethod.VNPay)
            {
                var payment = await _paymentRepository.GetNewestByOrderIdAndStatusAsync(request.OrderId, PaymentStatus.Paid, cancellationToken);
                if (payment == null)
                {
                    return "Order cannot be completed because payment is not paid.";
                }
                //Trừ kho bên vnpay return  rùi
            }
            else
            {
                var payment = await _paymentRepository.GetNewestByOrderIdAndStatusAsync(request.OrderId, PaymentStatus.Pending, cancellationToken);
                if (payment == null)
                {
                    return "No pending payment found for this order.";
                }
                payment.Status = (int)PaymentStatus.Paid;
                order.PaidAt = DateTime.UtcNow;
                //OrderPaidEvent → InventoryService (trừ kho)
                var orderPaidEvent = OrderToKafka.MapToOrderPaidEvent(payment.Order);
                _outboxRepository.AddEvent(orderPaidEvent, KafkaTopics.OrderPaid);

                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.SaveChangesAsync();
            }
            // Publish process Point loyalty event to Kafka
            await _kafkaProducer.PublishAsync(KafkaTopics.ProcessOrderReward, new ProcessOrderRewardEvent
            {
                Id = order.CustomerId ?? Guid.Empty,
                OrderAmount = order.PaidAmount ?? 0
            });
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
        orderEvent.Title = DictionaryOrderTileEmail.GetTitle(order.Status);
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
