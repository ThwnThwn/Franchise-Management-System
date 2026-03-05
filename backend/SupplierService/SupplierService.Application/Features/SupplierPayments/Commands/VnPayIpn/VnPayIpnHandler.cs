using System.Net;
using System.Text.Json;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplierService.Application.Helpers;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Features.SupplierPayments.Commands.VnPayIpn;

public sealed class VnPayIpnHandler
    : IRequestHandler<VnPayIpnCommand, Result<VnPayIpnResponse>>
{
    private readonly ISupplierPaymentRepository _paymentRepository;
    private readonly IPurchaseOrderRepository _poRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<VnPayIpnHandler> _logger;

    public VnPayIpnHandler(
        ISupplierPaymentRepository paymentRepository,
        IPurchaseOrderRepository poRepository,
        IConfiguration config,
        ILogger<VnPayIpnHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _poRepository = poRepository;
        _config = config;
        _logger = logger;
    }

    public async Task<Result<VnPayIpnResponse>> Handle(
        VnPayIpnCommand request,
        CancellationToken cancellationToken)
    {
        var vnpSection = _config.GetSection("VNPay");
        var hashSecret = vnpSection["HashSecret"];
        if (string.IsNullOrWhiteSpace(hashSecret))
        {
            _logger.LogError("VNPay HashSecret is not configured");
            return Result.Error("VNPay HashSecret is not configured");
        }

        var query = request.QueryParams;

        // 1. Kiểm tra chữ ký tổng thể
        if (!query.TryGetValue("vnp_SecureHash", out var vnpSecureHash))
        {
            return Result.Success(new VnPayIpnResponse { RspCode = "97", Message = "Invalid signature" });
        }

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
        _logger.LogDebug("[VNPay IPN] received  hash = {Received}", vnpSecureHash);
        _logger.LogDebug("[VNPay IPN] computed (raw)     = {Raw}", hashRaw);
        _logger.LogDebug("[VNPay IPN] computed (encoded) = {Enc}", hashEncoded);
        // ── END DEBUG ────────────────────────────────────────────────────────

        var isValidSignature = VNPayHelper.VerifySignature(hashSecret, vnpParams, vnpSecureHash);
        if (!isValidSignature)
        {
            _logger.LogWarning("[VNPay IPN] Signature verification FAILED");
            return Result.Success(new VnPayIpnResponse { RspCode = "97", Message = "Invalid signature" });
        }

        // 2. Đọc thông tin giao dịch
        query.TryGetValue("vnp_TxnRef", out var txnRef);
        query.TryGetValue("vnp_ResponseCode", out var responseCode);
        query.TryGetValue("vnp_TransactionNo", out var transactionNo);
        query.TryGetValue("vnp_Amount", out var amountStr);

        if (!Guid.TryParse(txnRef, out var paymentId))
        {
            return Result.Success(new VnPayIpnResponse { RspCode = "01", Message = "Order not found" });
        }

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            return Result.Success(new VnPayIpnResponse { RspCode = "01", Message = "Order not found" });
        }

        // 3. Kiểm tra trạng thái đã xử lý chưa (IPN có thể gọi lại nhiều lần)
        if (payment.Status == (int)PaymentStatus.Paid || payment.Status == (int)PaymentStatus.Failed)
        {
            _logger.LogInformation("[VNPay IPN] PaymentId {PaymentId} already processed. Skipping.", payment.Id);
            return Result.Success(new VnPayIpnResponse { RspCode = "02", Message = "Order already confirmed" });
        }

        // 4. Kiểm tra số tiền
        if (!long.TryParse(amountStr, out var vnpAmount))
        {
            return Result.Success(new VnPayIpnResponse { RspCode = "04", Message = "Invalid amount" });
        }

        var expected = (long)Math.Floor(payment.Amount) * 100;
        if (expected != vnpAmount)
        {
            _logger.LogWarning(
                "[VNPay IPN] Amount mismatch for PaymentId {PaymentId}: expected {Expected}, got {Got}",
                paymentId, expected, vnpAmount);

            payment.Status = (int)PaymentStatus.Failed;
            payment.Provider = "vnpay";
            payment.ProviderPayload = JsonSerializer.Serialize(query);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            return Result.Success(new VnPayIpnResponse { RspCode = "04", Message = "Invalid amount" });
        }

        // 5. Cập nhật trạng thái payment
        if (responseCode == "00")
        {
            payment.Status = (int)PaymentStatus.Paid;
            payment.PaidAt = DateTimeOffset.UtcNow;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);

            // Cập nhật PurchaseOrder paid amount & payment status
            var po = await _poRepository.GetByIdWithItemsAsync(payment.PurchaseOrderId, cancellationToken);
            if (po != null)
            {
                po.PaidAmount += payment.Amount;
                po.Status = PurchaseOrderStatus.ConfirmPending;
                po.PaymentStatus = po.PaidAmount >= po.NetAmount
                    ? PaymentStatus.Paid
                    : PaymentStatus.Partial;
                await _poRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "[VNPay IPN] Thanh toán thành công: PaymentId {PaymentId}, PO {PurchaseOrderId}, PO Status: {PoStatus}",
                    payment.Id, payment.PurchaseOrderId, po.PaymentStatus);
            }
        }
        else
        {
            payment.Status = (int)PaymentStatus.Failed;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);

            _logger.LogWarning(
                "[VNPay IPN] Giao dịch thất bại: PaymentId {PaymentId}, ResponseCode {ResponseCode}",
                payment.Id, responseCode);
        }

        await _paymentRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(new VnPayIpnResponse
        {
            RspCode = "00",
            Message = "Confirm Success"
        });
    }
}
