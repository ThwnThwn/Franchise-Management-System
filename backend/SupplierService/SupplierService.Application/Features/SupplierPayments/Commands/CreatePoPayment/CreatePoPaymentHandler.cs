using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using SupplierService.Application.Helpers;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Features.SupplierPayments.Commands.CreatePoPayment;

public sealed class CreatePoPaymentHandler : IRequestHandler<CreatePoPaymentCommand, Result<CreatePoPaymentResult>>
{
    private readonly IPurchaseOrderRepository _poRepository;
    private readonly ISupplierPaymentRepository _paymentRepository;
    private readonly IConfiguration _config;

    public CreatePoPaymentHandler(
        IPurchaseOrderRepository poRepository,
        ISupplierPaymentRepository paymentRepository,
        IConfiguration config)
    {
        _poRepository = poRepository;
        _paymentRepository = paymentRepository;
        _config = config;
    }

    public async Task<Result<CreatePoPaymentResult>> Handle(CreatePoPaymentCommand command, CancellationToken cancellationToken)
    {
        var po = await _poRepository.GetByIdWithItemsAsync(command.PurchaseOrderId, cancellationToken);
        if (po == null || po.IsDeleted)
            return Result.NotFound($"Purchase order '{command.PurchaseOrderId}' not found.");

        if (po.Status == PurchaseOrderStatus.Cancelled)
            return Result.Error("Cannot record payment for a cancelled purchase order.");

        if (po.PaymentStatus == PaymentStatus.Paid)
            return Result.Error("Purchase order is already fully paid.");

        var remaining = po.NetAmount - po.PaidAmount;
        if (remaining <= 0)
            return Result.Error("Purchase order balance is zero or negative.");

        var paymentAmount = remaining;
        var code = $"SPAY-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        var payment = new SupplierPayment
        {
            Id = Guid.NewGuid(),
            PurchaseOrderId = po.Id,
            Code = code,
            Amount = paymentAmount,
            Method = (int)command.Method,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        if (command.Method == SupplierPaymentMethod.Cash)
        {
            payment.Status = (int)PaymentStatus.Paid;
            payment.PaidAt = DateTimeOffset.UtcNow;

            await _paymentRepository.CreateAsync(payment, cancellationToken);

            // Update PO payment status
            po.PaidAmount += paymentAmount;
            po.PaymentStatus = po.PaidAmount >= po.NetAmount ? PaymentStatus.Paid : PaymentStatus.Partial;
            await _poRepository.SaveChangesAsync(cancellationToken);

            return Result.Success(new CreatePoPaymentResult { PaymentId = payment.Id, Code = payment.Code, Url = string.Empty });
        }

        // VNPay — status stays Pending until callback

        payment.Status = (int)PaymentStatus.Pending;

        var vnpSection = _config.GetSection("VNPay");
        var tmnCode = vnpSection["TmnCode"] ?? throw new InvalidOperationException("VNPay:TmnCode not configured");
        var hashSecret = vnpSection["HashSecret"] ?? throw new InvalidOperationException("VNPay:HashSecret not configured");
        var paymentUrl = vnpSection["PaymentUrl"] ?? throw new InvalidOperationException("VNPay:PaymentUrl not configured");
        var configReturnUrl = vnpSection["SupplierReturnUrl"] ?? throw new InvalidOperationException("VNPay:SupplierReturnUrl not configured");

        // VNPay chỉ chấp nhận số nguyên VND — bỏ phần lẻ xu trước khi × 100
        var amountVnd = (long)Math.Floor(paymentAmount) * 100;

        var parameters = new Dictionary<string, string>
        {
            ["vnp_Version"] = vnpSection["Version"] ?? "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = tmnCode,
            ["vnp_Amount"] = amountVnd.ToString(),
            ["vnp_CreateDate"] = DateTimeOffset.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = vnpSection["CurrCode"] ?? "VND",
            ["vnp_IpAddr"] = "127.0.0.1",
            ["vnp_Locale"] = vnpSection["Locale"] ?? "vn",
            ["vnp_OrderInfo"] = $"Thanh toan don nhap hang {po.Code}",
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = configReturnUrl,
            ["vnp_TxnRef"] = payment.Id.ToString()
        };

        var url = VNPayHelper.BuildPaymentUrl(paymentUrl, hashSecret, parameters);

        await _paymentRepository.CreateAsync(payment, cancellationToken);

        return Result.Success(new CreatePoPaymentResult { Url = url , Code = code, PaymentId = payment.Id});
    }
}

