using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using SalesService.Application.ClientInterface.Engagement;
using SalesService.Application.Dtos.Voucher;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Signals;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Infrastructure.Extensions;
using SharedKernel.Infrastructure.Kafka;

namespace SalesService.Application.Features.Payments.CreatePayment;

public sealed class CreatePaymentHandler
    : IRequestHandler<CreatePaymentCommand, Result<CreatePaymentRes>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IConfiguration _config;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxSignal _outboxSignal;
    private readonly IEngagementServiceClient _engagementServiceClient;

    public CreatePaymentHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IKafkaProducer kafkaProducer,
        IConfiguration config,
        IOutboxRepository outboxRepository,
        IOutboxSignal outboxSignal,
        IEngagementServiceClient engagementServiceClient)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _kafkaProducer = kafkaProducer;
        _config = config;
        _outboxRepository = outboxRepository;
        _outboxSignal = outboxSignal;
        _engagementServiceClient = engagementServiceClient;
    }

    public async Task<Result<CreatePaymentRes>> Handle(
        CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Requets;
        if (request.OrderId == Guid.Empty)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(request.OrderId),
                ErrorMessage = "OrderId is required."
            });
        }

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null || order.IsDeleted)
            return Result.NotFound($"Order '{request.OrderId}' not found.");
        if (order.Status == OrderStatus.Completed)
            return Result.Error("Order already paid.");
        // Check CusomterID in Order
        if(order.CustomerId == Guid.Empty)
        {
            return Result.Error("CustomerID is null");
        }
        // ================= Sale Order Items =================
        // Get User's Vouchers
        if (!string.IsNullOrWhiteSpace(request.VoucherDiscountCode) || !string.IsNullOrWhiteSpace(request.VoucherShipCode))
        {
            var vouchers = await _engagementServiceClient.GetMyVoucherAsync(order.CustomerId.Value);
            var voucherMap = vouchers.ToDictionary(v => v.Code, StringComparer.OrdinalIgnoreCase);
            Voucher? voucherDiscount = null;
            Voucher? voucherShip = null;
            if (!voucherMap.TryGetValue(request.VoucherDiscountCode, out voucherDiscount))
            {
                return Result.NotFound("Voucher Discount not found or not owned by user");
            }
            if (!voucherMap.TryGetValue(request.VoucherShipCode, out voucherShip))
            {
                return Result.NotFound("Voucher Ship not found or not owned by user");
            }

            // Voucher Discount 
            if(voucherDiscount.MinOrderValue <= order.Subtotal)
            {
                decimal? voucherDiscountAmount = 0;
                if(voucherDiscount.VoucherItems.Count > 0)
                {
                    var voucherItem = voucherDiscount.VoucherItems;
                    var requiredItemsVoucher = voucherDiscount.VoucherItems
                        .Where(v => v.Type == VoucherItemType.ITEM_REQUIRED)
                        .ToList();

                    foreach ( var requiredItem in requiredItemsVoucher)
                    {
                        var orderItem = order.OrderItems.Where(x => x.Id == requiredItem.VariantId).FirstOrDefault();
                        if(orderItem.Quantity < requiredItem.Quantity)
                        {
                            throw new Exception($"Voucher {voucherDiscount.Code} require {requiredItem.Quantity} {requiredItem.VariantId}");
                        }
                    }

                    var giftItems = voucherDiscount.VoucherItems
                        .Where(v => v.Type == VoucherItemType.ITEM_GIFT)
                        .ToList();
                    
                }
                else if (voucherDiscount.Type.Equals(VoucherType.DISCOUNT_FIXED_AMOUNT))
                {
                    voucherDiscountAmount = order.Subtotal * voucherDiscount.DiscountValue/100;
                    voucherDiscountAmount = voucherDiscountAmount > voucherDiscount.MaxDiscountValue ? voucherDiscount.MaxDiscountValue : voucherDiscountAmount;
                }else if (voucherDiscount.Type.Equals(VoucherType.DISCOUNT_FIXED_AMOUNT))
                {
                    voucherDiscountAmount = voucherDiscount.DiscountValue;
                }
            }
            
        }
        

        // ================= CREATE PAYMENT =================
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Code = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Amount = order.TotalAmount,
            Currency = "VND",
            Method = (int)request.PaymentMethod,// 0 = Cash | 1 = VNPay
            Status = (int)PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _paymentRepository.CreateAsync(payment, cancellationToken);

        // ==========================================================
        // ================= CASH FLOW ==============================
        // ==========================================================
        if (request.PaymentMethod == PaymentMethod.Cash)
        {
            // ===== UPDATE ORDER =====
            order.PaidAmount = order.TotalAmount;
            order.PaymentMethod = PaymentMethod.Cash;
            order.Status = OrderStatus.ConfirmationPending;
            order.PaidAt = DateTimeOffset.UtcNow;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _orderRepository.UpdateAsync(order, cancellationToken);

            // ===== OUTBOX EVENTS =====
            if (order.OrderItems.Any())
            {
                var orderStatusEvent = OrderToKafka.MapToOrderStatusEvent(order);
                _outboxRepository.AddEvent(orderStatusEvent, KafkaTopics.OrderNotifications);
                await _kafkaProducer.PublishAsync(KafkaTopics.OrderNotifications, orderStatusEvent);

                // 2. OrderPaidEvent → InventoryService (trừ kho)
                var orderPaidEvent = OrderToKafka.MapToOrderPaidEvent(order);
                _outboxRepository.AddEvent(orderPaidEvent, KafkaTopics.OrderPaid);
            }

            _outboxSignal.Pulse();

            return Result.Success(new CreatePaymentRes { Url = "" });
        }

        // ==========================================================
        // ================= VNPAY FLOW =============================
        // ==========================================================

        var vnpSection = _config.GetSection("VNPay");

        var paymentUrl = VNPayHelper.BuildPaymentUrl(
            vnpSection["PaymentUrl"]!,
            vnpSection["HashSecret"]!,
            BuildVNPayParams(vnpSection, order, payment, request.ReturnUrl)
        );

        return Result.Success(new CreatePaymentRes
        {
            Url = paymentUrl
        });
    }

    private Dictionary<string, string> BuildVNPayParams(
        IConfigurationSection vnpSection,
        Order order,
        Payment payment,
        string? returnUrl)
    {
        var callbackUrl = vnpSection["ReturnUrl"]!;

        if (!string.IsNullOrEmpty(returnUrl))
        {
            callbackUrl += $"?redirectTo={Uri.EscapeDataString(returnUrl)}";
        }

        return new Dictionary<string, string>
        {
            ["vnp_Version"] = vnpSection["Version"] ?? "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = vnpSection["TmnCode"]!,
            ["vnp_Amount"] = ((long)(order.TotalAmount * 100)).ToString(),
            ["vnp_CreateDate"] = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = "127.0.0.1",
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = $"Thanh toan don hang {order.Code}",
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = callbackUrl,
            ["vnp_TxnRef"] = payment.Id.ToString()
        };
    }
}