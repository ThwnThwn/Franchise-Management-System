using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Payments;

namespace SalesService.Application.Features.Payments.VnPayReturn;

public sealed class VnPayReturnHandler(
    IPaymentRepository paymentRepository,
    IConfiguration config,
    ILogger<VnPayReturnHandler> logger
) : IRequestHandler<VnPayReturnCommand, Result<VnPayReturnResponse>>
{
    public async Task<Result<VnPayReturnResponse>> Handle(
        VnPayReturnCommand request,
        CancellationToken cancellationToken)
    {
        var vnpSection = config.GetSection("VNPay");
        var hashSecret = vnpSection["HashSecret"];
        if (string.IsNullOrWhiteSpace(hashSecret))
            return Result.Error("VNPay HashSecret is not configured");

        var query = request.QueryParams;
        if (!query.TryGetValue("vnp_SecureHash", out var vnpSecureHash))
        {
            return Result.Invalid([new ValidationError { Identifier = "vnp_SecureHash", ErrorMessage = "Missing vnp_SecureHash" }]);
        }

        var vnpParams = new Dictionary<string, string>();
        foreach (var kv in query)
        {
            if (kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(kv.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(kv.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            {
                vnpParams[kv.Key] = kv.Value;
            }
        }

        var isValidSignature = VNPayHelper.VerifySignature(hashSecret, vnpParams, vnpSecureHash);
        if (!isValidSignature) return Result.Error("Invalid VNPay signature");

        query.TryGetValue("vnp_TxnRef", out var txnRef);
        query.TryGetValue("vnp_ResponseCode", out var responseCode);

        if (!Guid.TryParse(txnRef, out var paymentId))
        {
            return Result.Invalid([new ValidationError { Identifier = "vnp_TxnRef", ErrorMessage = "Invalid vnp_TxnRef" }]);
        }

        var payment = await paymentRepository.GetByIdWithOrderAsync(paymentId, cancellationToken);
        if (payment == null) return Result.NotFound("Payment not found");

        // CHỈ KIỂM TRA LOGIC CHO UI, KHÔNG LƯU DB HAY BẮN KAFKA Ở ĐÂY
        var isSuccess = responseCode == "00";

        if (isSuccess)
            logger.LogInformation("VNPay Return: Giao dịch thành công PaymentId {PaymentId}. Chờ IPN xử lý.", payment.Id);
        else
            logger.LogWarning("VNPay Return: Giao dịch thất bại PaymentId {PaymentId} (Mã: {Code}).", payment.Id, responseCode);

        return Result.Success(new VnPayReturnResponse
        {
            Success = isSuccess,
            PaymentId = payment.Id,
            OrderId = payment.OrderId ?? Guid.Empty,
            ResponseCode = responseCode ?? string.Empty
        });
    }
}