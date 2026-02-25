using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Signals;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Infrastructure.Extensions;

namespace SalesService.Application.Features.Payments.CreatePayment;

public sealed class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, Result<CreatePaymentRes>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IConfiguration _config;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxSignal _outboxSignal;

    public CreatePaymentHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IConfiguration config,
        IOutboxRepository outboxRepository,
        IOutboxSignal outboxSignal)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _config = config;
        _outboxRepository = outboxRepository;
        _outboxSignal = outboxSignal;
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
        if (order == null || order.IsDeleted == true)
        {
            return Result.NotFound($"Order with id '{request.OrderId}' not found.");
        }

        // Nếu order đã được đánh dấu Completed thì không cho tạo payment mới
        if (order.Status == OrderStatus.Completed) // 4 = Completed
        {
            return Result.Error("Order already paid.");
        }

        // Tạo payment mới cho order
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Code = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Amount = order.TotalAmount,
            Currency = "VND",
            Method = (int)request.PaymentMethod,// 0 = Cash | 1 = VNPay
            Status = request.PaymentMethod == PaymentMethod.VNPay
                        ? (int)PaymentStatus.Pending
                        : (int)PaymentStatus.Paid,      // if VNPay then Paid Else Pending
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _paymentRepository.CreateAsync(payment, cancellationToken);
        if (payment.Method == 0) // Cash
        {
            //Mark Order as Paid
            order.PaidAmount = order.TotalAmount;
            order.PaymentMethod = (int)PaymentMethod.Cash;
            order.Status = OrderStatus.ConfirmationPending;
            order.UpdatedAt = DateTime.UtcNow;

            // Publish events qua Outbox
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                // 1. OrderStatusEvent → NotificationService
                var orderStatusEvent = OrderToKafka.MapToOrderStatusEvent(order);
                _outboxRepository.AddEvent(orderStatusEvent, KafkaTopics.OrderNotifications);

                // 2. OrderPaidEvent → InventoryService (trừ kho)
                var orderPaidEvent = OrderToKafka.MapToOrderPaidEvent(order);
                _outboxRepository.AddEvent(orderPaidEvent, KafkaTopics.OrderPaid);
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);

            // Pulse Worker
            _outboxSignal.Pulse();

            return Result.Success(new CreatePaymentRes
            {
                Url = ""
            });
        }
        var vnpSection = _config.GetSection("VNPay");
        var tmnCode = vnpSection["TmnCode"] ?? throw new InvalidOperationException("VNPay:TmnCode is not configured");
        var hashSecret = vnpSection["HashSecret"] ?? throw new InvalidOperationException("VNPay:HashSecret is not configured");
        var paymentUrl = vnpSection["PaymentUrl"] ?? throw new InvalidOperationException("VNPay:PaymentUrl is not configured");
        var configReturnUrl = vnpSection["ReturnUrl"] ?? throw new InvalidOperationException("VNPay:ReturnUrl is not configured");

        // Encode custom returnUrl from request into callback URL if provided
        var callbackUrl = configReturnUrl;
        if (!string.IsNullOrEmpty(request.ReturnUrl))
        {
            var encodedReturnUrl = Uri.EscapeDataString(request.ReturnUrl);
            callbackUrl = $"{configReturnUrl}?redirectTo={encodedReturnUrl}";
        }

        var amountVnd = (long)(order.TotalAmount * 100); // VNPay dùng đơn vị x100

        var parameters = new Dictionary<string, string>
        {
            ["vnp_Version"] = vnpSection["Version"] ?? "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = tmnCode,
            ["vnp_Amount"] = amountVnd.ToString(),
            ["vnp_CreateDate"] = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = vnpSection["CurrCode"] ?? "VND",
            ["vnp_IpAddr"] = "127.0.0.1",
            ["vnp_Locale"] = vnpSection["Locale"] ?? "vn",
            ["vnp_OrderInfo"] = $"Thanh toan don hang {order.Code}",
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = callbackUrl,  // Use callbackUrl with encoded returnUrl
            ["vnp_TxnRef"] = payment.Id.ToString()
        };

        var url = VNPayHelper.BuildPaymentUrl(paymentUrl, hashSecret, parameters);

        return Result.Success(new CreatePaymentRes
        {
            Url = url
        });
    }
}

