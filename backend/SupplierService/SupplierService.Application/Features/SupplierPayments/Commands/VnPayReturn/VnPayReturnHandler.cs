using System.Net;
using System.Text.Json;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplierService.Application.Helpers;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Features.SupplierPayments.Commands.VnPayReturn;

public sealed class VnPayReturnHandler : IRequestHandler<VnPayReturnCommand, Result<VnPayReturnResult>>
{
    private readonly ISupplierPaymentRepository _paymentRepository;
    private readonly IPurchaseOrderRepository _poRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<VnPayReturnHandler> _logger;

    public VnPayReturnHandler(
        ISupplierPaymentRepository paymentRepository,
        IPurchaseOrderRepository poRepository,
        IConfiguration config,
        ILogger<VnPayReturnHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _poRepository = poRepository;
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

        // ── DEBUG: so sánh hash để tìm nguyên nhân mismatch ──────────────────
        var sortedDebug = new SortedDictionary<string, string>(vnpParams, StringComparer.Ordinal);
        // cách 1: raw (không encode)
        var rawQuery = string.Join("&", sortedDebug.Select(kv => $"{kv.Key}={kv.Value}"));
        // cách 2: url-encode từng giá trị
        var encodedQuery = string.Join("&", sortedDebug.Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));
        var hashRaw = VNPayHelper.HmacSHA512(hashSecret, rawQuery);
        var hashEncoded = VNPayHelper.HmacSHA512(hashSecret, encodedQuery);
        _logger.LogDebug("[VNPay] received  hash = {Received}", vnpSecureHash);
        _logger.LogDebug("[VNPay] computed (raw)     = {Raw}", hashRaw);
        _logger.LogDebug("[VNPay] computed (encoded) = {Enc}", hashEncoded);
        _logger.LogDebug("[VNPay] raw_match={RawMatch}  encoded_match={EncMatch}",
            string.Equals(hashRaw, vnpSecureHash, StringComparison.OrdinalIgnoreCase),
            string.Equals(hashEncoded, vnpSecureHash, StringComparison.OrdinalIgnoreCase));
        // ── END DEBUG ──────────────────────────────────────────────────────────

        // xác thực chữ ký
        var isValidSignature = VNPayHelper.VerifySignature(hashSecret, vnpParams, vnpSecureHash);
        if (!isValidSignature)
        {
            _logger.LogWarning("[VNPay] Signature verification FAILED. raw_match={R} encoded_match={E}",
                string.Equals(hashRaw, vnpSecureHash, StringComparison.OrdinalIgnoreCase),
                string.Equals(hashEncoded, vnpSecureHash, StringComparison.OrdinalIgnoreCase));
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

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            return Result.NotFound("Payment not found");
        }

        // xác thực số tiền
        if (long.TryParse(amountStr, out var vnpAmount))
        {
            var expected = (long)Math.Floor(payment.Amount) * 100;
            if (expected != vnpAmount)
            {
                _logger.LogWarning(
                    "VNPay amount mismatch for Payment {PaymentId}: expected {Expected}, got {Got}",
                    paymentId, expected, vnpAmount);

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
            payment.PaidAt = DateTimeOffset.UtcNow;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);

            // cập nhật PO paid amount & payment status
            var po = await _poRepository.GetByIdWithItemsAsync(payment.PurchaseOrderId, cancellationToken);
            if (po != null)
            {
                po.PaidAmount += payment.Amount;
                po.PaymentStatus = po.PaidAmount >= po.NetAmount
                    ? PaymentStatus.Paid
                    : PaymentStatus.Partial;
                await _poRepository.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation(
                "VNPay callback success: Payment {PaymentId}, PO {PurchaseOrderId}, TxnNo {TxnNo}",
                payment.Id, payment.PurchaseOrderId, transactionNo);
        }
        else
        {
            payment.Status = (int)PaymentStatus.Failed;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);

            _logger.LogWarning(
                "VNPay callback failed: Payment {PaymentId}, ResponseCode {ResponseCode}",
                payment.Id, responseCode);
        }

        await _paymentRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(new VnPayReturnResult
        {
            Success = responseCode == "00",
            PaymentId = payment.Id,
            PurchaseOrderId = payment.PurchaseOrderId,
            ResponseCode = responseCode ?? string.Empty
        });
    }
}
