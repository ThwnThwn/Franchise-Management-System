using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Payments.CreateVnPayUrl;

public sealed class CreateVnPayUrlHandler
    : IRequestHandler<CreateVnPayUrlCommand, Result<CreateVnPayUrlResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IConfiguration _config;

    public CreateVnPayUrlHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IConfiguration config)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _config = config;
    }

    public async Task<Result<CreateVnPayUrlResponse>> Handle(
        CreateVnPayUrlCommand request,
        CancellationToken cancellationToken)
    {
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

        // Nếu order đã được đánh dấu Paid thì không cho tạo payment mới
        if (order.Status == OrderStatus.Paid) // 1 = Paid
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
            Method = 1,      // 1 = VNPay
            Status = 0,      // 0 = Pending
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _paymentRepository.CreateAsync(payment, cancellationToken);

        var vnpSection = _config.GetSection("VNPay");
        var tmnCode = vnpSection["TmnCode"] ?? throw new InvalidOperationException("VNPay:TmnCode is not configured");
        var hashSecret = vnpSection["HashSecret"] ?? throw new InvalidOperationException("VNPay:HashSecret is not configured");
        var paymentUrl = vnpSection["PaymentUrl"] ?? throw new InvalidOperationException("VNPay:PaymentUrl is not configured");
        var returnUrl = vnpSection["ReturnUrl"] ?? throw new InvalidOperationException("VNPay:ReturnUrl is not configured");

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
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_TxnRef"] = payment.Id.ToString()
        };

        var url = VNPayHelper.BuildPaymentUrl(paymentUrl, hashSecret, parameters);

        return Result.Success(new CreateVnPayUrlResponse
        {
            PaymentUrl = url
        });
    }
}

