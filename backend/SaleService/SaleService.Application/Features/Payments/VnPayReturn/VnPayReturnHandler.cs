
using System.Text.Json;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Signals;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Extensions;

namespace SalesService.Application.Features.Payments.VnPayReturn;

public sealed class VnPayReturnHandler
    : IRequestHandler<VnPayReturnCommand, Result<VnPayReturnResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<VnPayReturnHandler> _logger;

    public VnPayReturnHandler(
        IPaymentRepository paymentRepository,
        IConfiguration config,
        IOutboxRepository outboxRepository,
        IOutboxSignal outboxSignal,
        ILogger<VnPayReturnHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _config = config;
        _logger = logger;
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

        // Chỉ kiểm tra logic để hiển thị cho UI (KHÔNG LƯU DB, KHÔNG BẮN KAFKA)
        // Mọi việc cập nhật dữ liệu để VnPayIpnHandler lo gánh
        var isSuccess = responseCode == "00";

        if (isSuccess)
        {
            _logger.LogInformation("VNPay Return: Giao dịch thành công cho PaymentId {PaymentId}. Chờ IPN xử lý DB.", payment.Id);
        }
        else
        {
            _logger.LogWarning("VNPay Return: Giao dịch thất bại cho PaymentId {PaymentId} (Mã: {ResponseCode}).", payment.Id, responseCode);
        }

        return Result.Success(new VnPayReturnResponse
        {
            Success = isSuccess,
            PaymentId = payment.Id,
            OrderId = payment.OrderId.Value,
            ResponseCode = responseCode ?? string.Empty
        });
    }

}

