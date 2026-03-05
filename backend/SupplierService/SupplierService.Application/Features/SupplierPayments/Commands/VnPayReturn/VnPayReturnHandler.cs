using System.Net;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplierService.Application.Helpers;
using SupplierService.Application.Repositories;

namespace SupplierService.Application.Features.SupplierPayments.Commands.VnPayReturn;

public sealed class VnPayReturnHandler : IRequestHandler<VnPayReturnCommand, Result<VnPayReturnResult>>
{
    private readonly ISupplierPaymentRepository _paymentRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<VnPayReturnHandler> _logger;

    public VnPayReturnHandler(
        ISupplierPaymentRepository paymentRepository,
        IConfiguration config,
        ILogger<VnPayReturnHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _config = config;
        _logger = logger;
    }

    public async Task<Result<VnPayReturnResult>> Handle(
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

        // ── DEBUG: so sánh hash ─────────────────────────────────────────────
        var sortedDebug = new SortedDictionary<string, string>(vnpParams, StringComparer.Ordinal);
        var rawQuery = string.Join("&", sortedDebug.Select(kv => $"{kv.Key}={kv.Value}"));
        var encodedQuery = string.Join("&", sortedDebug.Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));
        var hashRaw = VNPayHelper.HmacSHA512(hashSecret, rawQuery);
        var hashEncoded = VNPayHelper.HmacSHA512(hashSecret, encodedQuery);
        _logger.LogDebug("[VNPay Return] received  hash = {Received}", vnpSecureHash);
        _logger.LogDebug("[VNPay Return] computed (raw)     = {Raw}", hashRaw);
        _logger.LogDebug("[VNPay Return] computed (encoded) = {Enc}", hashEncoded);
        _logger.LogDebug("[VNPay Return] raw_match={RawMatch}  encoded_match={EncMatch}",
            string.Equals(hashRaw, vnpSecureHash, StringComparison.OrdinalIgnoreCase),
            string.Equals(hashEncoded, vnpSecureHash, StringComparison.OrdinalIgnoreCase));
        // ── END DEBUG ────────────────────────────────────────────────────────

        var isValidSignature = VNPayHelper.VerifySignature(hashSecret, vnpParams, vnpSecureHash);
        if (!isValidSignature)
        {
            _logger.LogWarning("[VNPay Return] Signature verification FAILED.");
            return Result.Error("Invalid VNPay signature");
        }

        query.TryGetValue("vnp_TxnRef", out var txnRef);
        query.TryGetValue("vnp_ResponseCode", out var responseCode);

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

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            return Result.NotFound("Payment not found");
        }

        // Chỉ kiểm tra kết quả để hiển thị cho UI (KHÔNG LƯU DB, KHÔNG CẬP NHẬT PO)
        // Mọi việc cập nhật dữ liệu để VnPayIpnHandler lo gánh
        var isSuccess = responseCode == "00";

        if (isSuccess)
        {
            _logger.LogInformation("[VNPay Return] Giao dịch thành công cho PaymentId {PaymentId}. Chờ IPN xử lý DB.", payment.Id);
        }
        else
        {
            _logger.LogWarning("[VNPay Return] Giao dịch thất bại cho PaymentId {PaymentId} (Mã: {ResponseCode}).", payment.Id, responseCode);
        }

        return Result.Success(new VnPayReturnResult
        {
            Success = isSuccess,
            PaymentId = payment.Id,
            PurchaseOrderId = payment.PurchaseOrderId,
            ResponseCode = responseCode ?? string.Empty
        });
    }
}
