<<<<<<< HEAD
﻿using Ardalis.Result;
=======
using System.Text.Json;
using Ardalis.Result;
>>>>>>> main
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Signals;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using System.Text.Json;

namespace SalesService.Application.Features.Payments.VnPayReturn;

public sealed class VnPayReturnHandler
    : IRequestHandler<VnPayReturnCommand, Result<VnPayReturnResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IConfiguration _config;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxSignal _outboxSignal;
    private readonly ILogger<VnPayReturnHandler> _logger;
    private readonly IKafkaProducer _kafkaProducer;

    public VnPayReturnHandler(
        IPaymentRepository paymentRepository, 
        IConfiguration config,
        IOutboxRepository outboxRepository,
        IOutboxSignal outboxSignal,
        ILogger<VnPayReturnHandler> logger,
        IKafkaProducer kafkaProducer)
    {
        _paymentRepository = paymentRepository;
        _config = config;
        _outboxRepository = outboxRepository;
        _outboxSignal = outboxSignal;
        _logger = logger;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<Result<VnPayReturnResponse>> Handle(
        VnPayReturnCommand request,
        CancellationToken cancellationToken)
    {
        var vnpSection = _config.GetSection("VNPay");
        var hashSecret = vnpSection["HashSecret"];
        if (string.IsNullOrWhiteSpace(hashSecret))
        {
            return Result.Error("VNPay HashSecret is not configured");
        }

        var query = request.QueryParams;
        if (!query.TryGetValue("vnp_SecureHash", out var vnpSecureHash))
        {
            return Result.Invalid(new[]
            {
                new ValidationError
                {
                    Identifier = "vnp_SecureHash",
                    ErrorMessage = "Missing vnp_SecureHash"
                }
            });
        }

        // lấy tất cả tham số bắt đầu với vnp_ để kiểm tra chữ ký
        var vnpParams = new Dictionary<string, string>();
        foreach (var kv in query)
        {
            if (!kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.Equals(kv.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(kv.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                continue;

            vnpParams[kv.Key] = kv.Value;
        }

        // xác thực chữ ký
        var isValidSignature = VNPayHelper.VerifySignature(hashSecret, vnpParams, vnpSecureHash);
        if (!isValidSignature)
        {
            return Result.Error("Invalid VNPay signature");
        }

        query.TryGetValue("vnp_TxnRef", out var txnRef);
        query.TryGetValue("vnp_ResponseCode", out var responseCode);
        query.TryGetValue("vnp_TransactionNo", out var transactionNo);
        query.TryGetValue("vnp_Amount", out var amountStr);

        if (!Guid.TryParse(txnRef, out var paymentId))
        {
            return Result.Invalid(new[]
            {
                new ValidationError
                {
                    Identifier = "vnp_TxnRef",
                    ErrorMessage = "Invalid vnp_TxnRef"
                }
            });
        }

        var payment = await _paymentRepository.GetByIdWithOrderAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            return Result.NotFound("Payment not found");
        }

        // xác thực số tiền
        if (long.TryParse(amountStr, out var vnpAmount))
        {
            var expected = (long)(payment.Amount * 100);
            if (expected != vnpAmount)
            {
                await AddPaymentEvent(payment.Id, "VNPAY_AMOUNT_MISMATCH", query, cancellationToken);

                payment.Status = (int)PaymentStatus.Failed;
                payment.Provider = "vnpay";
                payment.ProviderPayload = JsonSerializer.Serialize(query);

                await _paymentRepository.SaveChangesAsync(cancellationToken);

                return Result.Error("Amount mismatch");
            }
        }

        // cập nhật trạng thái payment
        if (responseCode == "00") // 00 = Thanh toán thành công
        {
            payment.Status = (int)PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);

            if (payment.Order != null)
            {
                payment.Order.Status = OrderStatus.ConfirmationPending;
                payment.Order.PaidAt = DateTimeOffset.UtcNow;
                payment.Order.PaidAmount = payment.Amount;
                payment.Order.PaymentMethod = PaymentMethod.VNPay;

                // PUBLISH OrderPaidEvent
                if (payment.Order.OrderItems != null && payment.Order.OrderItems.Any())
                {
                    var orderEvent = new OrderStatusEvent
                    {
                        OrderId = payment.Order.Id,
                        OrderCode = payment.Order.Code,
                        CustomerId = payment.Order.CustomerId,
                        CustomerEmail = string.Empty, // Will be fetched by Consumer
                        NewOrderStatus = OrderToKafka.MapOrderStatus(payment.Order.Status),
                        Items = OrderToKafka.MapOrderItems(payment.Order.OrderItems),
                        OrderSummary = $"Order {payment.Order.Code} created with {payment.Order.OrderItems.Count} items",
                        Tax = payment.Order.Tax,
                        TotalAmount = payment.Order.TotalAmount,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    var outbox = new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        Type = KafkaTopics.OrderNotifications,
                        Content = JsonSerializer.Serialize(orderEvent),
                        OccurredOn = DateTimeOffset.UtcNow
                    };
                    _outboxRepository.Add(outbox);
                    _logger.LogInformation("Published OrderPaidEvent for Order {OrderId}", payment.Order.Id);

                    // Publish order notification event to Kafka
                    await _kafkaProducer.PublishAsync(KafkaTopics.OrderNotifications, orderEvent);
                }
            }

            await AddPaymentEvent(payment.Id, "VNPAY_CALLBACK_SUCCESS", query, cancellationToken);
        }
        else
        {
            payment.Status = (int)PaymentStatus.Failed;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);

            if (payment.Order != null)
            {
                 // PUBLISH OrderCancelledEvent (release reservation)
                 var cancelledEvent = new SharedKernel.Events.OrderCancelledEvent
                 {
                     OrderId = payment.Order.Id,
                     CancelledAt = DateTimeOffset.UtcNow,
                     Reason = $"Payment Failed: {responseCode}"
                 };

                 var outbox = new OutboxMessage
                 {
                     Id = Guid.NewGuid(),
                     Type = KafkaTopics.OrderCancelled,
                     Content = JsonSerializer.Serialize(cancelledEvent),
                     OccurredOn = DateTimeOffset.UtcNow
                 };
                 _outboxRepository.Add(outbox);
            }

            await AddPaymentEvent(payment.Id, "VNPAY_CALLBACK_FAILED", query, cancellationToken);
        }

        await _paymentRepository.SaveChangesAsync(cancellationToken);
        
        // Pulse Outbox Worker
        _outboxSignal.Pulse();

        return Result.Success(new VnPayReturnResponse
        {
            Success = responseCode == "00",
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            ResponseCode = responseCode ?? string.Empty
        });
    }

    private async Task AddPaymentEvent(
        Guid paymentId,
        string eventType,
        IDictionary<string, string> query,
        CancellationToken cancellationToken)
    {
        var payloadJson = JsonSerializer.Serialize(query);

        var evt = new PaymentEvent
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            EventType = eventType,
            Payload = payloadJson,
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddEventAsync(evt, cancellationToken);
    }
}

