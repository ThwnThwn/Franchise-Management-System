using MediatR;
using Microsoft.Extensions.Logging;
using SalesService.Application.AppExceptions;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Extensions;
using SharedKernel.Infrastructure.Kafka;

namespace SalesService.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler(
    IUnitOfWork uow,
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IOutboxRepository outboxRepository,
    ILogger<UpdateOrderStatusCommandHandler> logger,
    IKafkaProducer kafkaProducer,
    IInventoryServiceClient inventoryServiceClient,
    IWalletRepository walletRepository,
    IWalletTransactionRepository transactionRepo
) : IRequestHandler<UpdateOrderStatusCommand, string>
{
    public async Task<string> Handle(UpdateOrderStatusCommand commnad, CancellationToken cancellationToken)
    {
        var request = commnad.Request;

        //Validate OrderIdw
        if (request.OrderId == Guid.Empty)
        {
            throw new KeyNotFoundException("OrderId is required.");
        }

        //Load Order
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null || order.IsDeleted)
        {
            throw new KeyNotFoundException($"Order with id '{request.OrderId}' not found.");
        }

        if (!OrderStatusTransitionPolicy.CanTransition(order.Status, request.Status))
        {
            throw new BusinessException($"Invalid order status transition from {order.Status} to {request.Status}");
        }

        await uow.BeginTransactionAsync(cancellationToken);
        try
        {
            #region Order Cancelled

            if (request.Status == OrderStatus.Cancelled)
            {
                await inventoryServiceClient.ReleaseStockAsync(new ReleaseStockReq
                {
                    OrderId = order.Id,
                    StoreId = order.StoreId!.Value
                });
                if (order.Status == OrderStatus.ConfirmationPending)
                {
                    if (order.CustomerId == null)
                    {
                        throw new BusinessException("Đơn này không thể huỷ do khách vãng lai");
                    }

                    //Hoàn về wallet
                    var wallet = await walletRepository.GetByUserIdAsync(order.CustomerId.Value, cancellationToken);
                    if (wallet == null || wallet.RowVersion == null)
                    {
                        throw new KeyNotFoundException("Cancel fail due to no wallet found");
                    }

                    decimal walletAmount = order.OrderSaleItems
                            .Where(x => x.Type == OrderSaleItemType.Wallet)
                            .Select(x => x.Amount)
                            .FirstOrDefault();
                    if (order.PaidAmount == null)
                    {
                        throw new BusinessException($"Cancel fail due to no PaidAmout found in order with ID {order.Id}");
                    }

                    decimal amount = walletAmount;
                    if (order.PaymentMethod == PaymentMethod.VNPay)
                    {
                        amount += order.PaidAmount.Value;
                    }

                    var withdrawTransaction = new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = order.CustomerId.Value,
                        WalletId = wallet.Id,
                        TransactionCode = $"WDR-{DateTime.UtcNow:yyyyMMddHHmmss}",
                        Amount = amount,
                        TransactionType = TransactionType.Withdrawal,
                        PaymentMethod = order.PaymentMethod,
                        Status = TransactionStatus.Completed,
                        ReferenceType = WalletReferenceType.Withdrawal,
                        Notes = "Order Refund",
                        Description = $"Hoàn tiền cho huỷ đơn hàng {order.Code}",
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        BalanceBefore = wallet.Balance,
                        BalanceAfter = wallet.Balance + amount
                    };

                    transactionRepo.Add(withdrawTransaction);
                    walletRepository.ApplyAddBalance(wallet, amount, wallet.RowVersion);

                    if (request.RoleSlug == "staff")
                    {
                        // Hoàn Point + Voucher
                        decimal point = order.OrderSaleItems
                            .Where(x => x.Type == OrderSaleItemType.LoyaltyPoint)
                            .Select(x => x.Amount)
                            .FirstOrDefault();

                        if (point != 0)
                        {
                            UpdatePointEvent userPoint = new()
                            {
                                Id = order.CustomerId.Value,
                                OrderId = order.Id,
                                Point = point,
                                ActionType = UpdatePointType.Add
                            };
                            await kafkaProducer.PublishAsync(KafkaTopics.UpdateUserPoint, userPoint);
                        }

                        // Kakfa Event Voucher
                        ReverseVoucherEvent userVoucher = new()
                        {
                            OrderId = order.Id
                        };
                        await kafkaProducer.PublishAsync(KafkaTopics.ReverseUserVoucher, userVoucher);
                    }
                }
            }

            #endregion Order Cancelled

            #region Order Completed

            if (request.Status == OrderStatus.Completed)
            {
                if (order.PaymentMethod == PaymentMethod.VNPay)
                {
                    var payment = await paymentRepository.GetNewestByOrderIdAndStatusAsync(request.OrderId, PaymentStatus.Paid, cancellationToken)
                        ?? throw new BusinessException("Order cannot be completed because payment is not paid.");
                }
                else
                {
                    var payment = await paymentRepository.GetNewestByOrderIdAndStatusAsync(request.OrderId, PaymentStatus.Pending, cancellationToken)
                        ?? throw new BusinessException("No pending payment found for this order.");

                    payment.Status = (int)PaymentStatus.Paid;
                    payment.UpdatedAt = DateTime.UtcNow;
                    order.PaidAt = DateTime.UtcNow;

                    //OrderPaidEvent → InventoryService (trừ kho)
                    var orderPaidEvent = OrderToKafka.MapToOrderPaidEvent(payment.Order);
                    outboxRepository.AddEvent(orderPaidEvent, KafkaTopics.OrderPaid);
                }
                // Publish process Point loyalty event to Kafka
                await kafkaProducer.PublishAsync(KafkaTopics.ProcessOrderReward, new ProcessOrderRewardEvent
                {
                    Id = order.CustomerId ?? Guid.Empty,
                    OrderAmount = order.PaidAmount ?? 0
                });
            }

            #endregion Order Completed

            order.Status = request.Status;
            order.UpdatedAt = DateTime.UtcNow;

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
                CreatedAt = DateTimeOffset.UtcNow,
                Title = DictionaryOrderTileEmail.GetTitle(order.Status)
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
            outboxRepository.Add(outboxMessage);
            logger.LogInformation("Added OrderCreatedEvent to Outbox for Order {OrderCode}", order.Code);

            await uow.CommitAsync(cancellationToken);

            // Publish order notification event to Kafka
            await kafkaProducer.PublishAsync(KafkaTopics.OrderNotifications, orderEvent);

            return $"Order status updated successfully to '{request.Status}'";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Transaction failed while updating status for Order {OrderId}", request.OrderId);
            await uow.RollbackAsync(cancellationToken);
            throw;
        }
    }
}