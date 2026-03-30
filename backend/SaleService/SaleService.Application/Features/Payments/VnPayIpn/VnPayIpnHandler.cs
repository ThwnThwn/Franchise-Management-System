using System.Text.Json;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Signals;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Extensions;

namespace SalesService.Application.Features.Payments.VnPayIpn;

public sealed class VnPayIpnHandler
    : IRequestHandler<VnPayIpnCommand, Result<VnPayIpnResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IConfiguration _config;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxSignal _outboxSignal;
    private readonly ILogger<VnPayIpnHandler> _logger;

    public VnPayIpnHandler(
        IPaymentRepository paymentRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IConfiguration config,
        IOutboxRepository outboxRepository,
        IOutboxSignal outboxSignal,
        ILogger<VnPayIpnHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _config = config;
        _outboxRepository = outboxRepository;
        _outboxSignal = outboxSignal;
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
        if (!query.TryGetValue("vnp_SecureHash", out var vnpSecureHash))
        {
            return Result.Success(new VnPayIpnResponse { RspCode = "97", Message = "Invalid signature" });
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
        
        // muốn test trên grpc xem hàm viết đúng không 
        // thì cmt lại cái check valid này lại 
        
        if (!isValidSignature)
        {
            return Result.Success(new VnPayIpnResponse { RspCode = "97", Message = "Invalid signature" });
        }

        query.TryGetValue("vnp_TxnRef", out var txnRef);
        query.TryGetValue("vnp_ResponseCode", out var responseCode);
        query.TryGetValue("vnp_TransactionNo", out var transactionNo);
        query.TryGetValue("vnp_Amount", out var amountStr);

        if (!Guid.TryParse(txnRef, out var paymentId))
        {
            return Result.Success(new VnPayIpnResponse { RspCode = "01", Message = "Order not found" });
        }

        // Dùng GetByIdAsync thay vì GetByIdWithOrderAsync để đảm bảo tìm thấy Payment 
        // ngay cả khi không có Order (ví dụ: nạp tiền ví).
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            _logger.LogWarning("[IPN] Payment {PaymentId} not found", paymentId);
            throw new Exception("payment is null |VnpayHandler");
        }
        // ── PHÂN NHÁNH: Wallet Deposit vs Order ──────────────────────────────
        if (payment.ReferenceType == PaymentReferenceType.Deposit && payment.ReferenceId.HasValue)
        {
            return await HandleWalletDepositIpnAsync(payment, responseCode, transactionNo, query, cancellationToken);
        }
        // Kiểm tra xem Payment đã được cập nhật trước đó chưa (IPN có thể gọi nhiều lần)
        if (payment.Status == (int)PaymentStatus.Paid || payment.Status == (int)PaymentStatus.Failed)
        {
            return Result.Success(new VnPayIpnResponse { RspCode = "02", Message = "Order already confirmed" });
        }

        // Nếu là thanh toán đơn hàng (không phải nạp ví) và có OrderId, load thêm Order
        if (payment.ReferenceType != PaymentReferenceType.Deposit && payment.OrderId.HasValue)
        {
            payment = await _paymentRepository.GetByIdWithOrderAsync(paymentId, cancellationToken);
        }
        
        // xác thực số tiền
        if (long.TryParse(amountStr, out var vnpAmount))
        {
            var expected = (long)(payment!.Amount * 100);
            if (expected != vnpAmount)
            {
                await AddPaymentEvent(payment.Id, "VNPAY_IPN_AMOUNT_MISMATCH", query, cancellationToken);

                payment.Status = (int)PaymentStatus.Failed;
                payment.Provider = "vnpay";
                payment.ProviderPayload = JsonSerializer.Serialize(query);

                await _paymentRepository.SaveChangesAsync(cancellationToken);

                return Result.Success(new VnPayIpnResponse { RspCode = "04", Message = "Invalid amount" });
            }
        }
        else
        {
            return Result.Success(new VnPayIpnResponse { RspCode = "04", Message = "Invalid amount" });
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
                    // 1. OrderStatusEvent → NotificationService (thông báo khách hàng)
                    var orderStatusEvent = OrderToKafka.MapToOrderStatusEvent(payment.Order);
                    _outboxRepository.AddEvent(orderStatusEvent, KafkaTopics.OrderNotifications);

                    // 2. OrderPaidEvent → InventoryService (trừ kho)
                    var orderPaidEvent = OrderToKafka.MapToOrderPaidEvent(payment.Order);
                    _outboxRepository.AddEvent(orderPaidEvent, KafkaTopics.OrderPaid);

                    _logger.LogInformation("Outbox: OrderStatusEvent + OrderPaidEvent cho Order {OrderId} via IPN", payment.Order.Id);
                }
            }

            await AddPaymentEvent(payment.Id, "VNPAY_IPN_SUCCESS", query, cancellationToken);
        }
        else
        {
            payment.Status = (int)PaymentStatus.Failed;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);

            if (payment.Order != null)
            {
                // OrderCancelledEvent → InventoryService (giải phóng giữ chỗ)
                var cancelledEvent = new OrderCancelledEvent
                {
                    OrderId = payment.Order.Id,
                    CancelledAt = DateTimeOffset.UtcNow,
                    Reason = $"Payment Failed via IPN: {responseCode}"
                };

                _outboxRepository.AddEvent(cancelledEvent, KafkaTopics.OrderCancelled);
            }

            await AddPaymentEvent(payment.Id, "VNPAY_IPN_FAILED", query, cancellationToken);
        }

        await _paymentRepository.SaveChangesAsync(cancellationToken);

        // Pulse Outbox Worker
        _outboxSignal.Pulse();

        return Result.Success(new VnPayIpnResponse
        {
            RspCode = "00",
            Message = "Confirm Success"
        });
    }

    // Wallet Deposit IPN handler
    private async Task<Result<VnPayIpnResponse>> HandleWalletDepositIpnAsync(
        Payment payment,
        string? responseCode,
        string? transactionNo,
        IDictionary<string, string> query,
        CancellationToken cancellationToken)
    {
        // Lấy WalletTransaction tham chiếu từ payment
        var walletTxn = await _walletTransactionRepository
            .GetByIdAsync(payment.ReferenceId!.Value, cancellationToken);

        if (walletTxn == null)
        {
            _logger.LogError("[WalletIPN] WalletTransaction not found for PaymentId {PaymentId}", payment.Id);
            return Result.Success(new VnPayIpnResponse { RspCode = "01", Message = "Wallet transaction not found" });
        }

        var wallet = await _walletRepository.GetByIdAsync(walletTxn.WalletId!.Value, cancellationToken);
        if (wallet == null)
        {
            _logger.LogError("[WalletIPN] Wallet not found for WalletId {WalletId}", walletTxn.WalletId);
            return Result.Success(new VnPayIpnResponse { RspCode = "01", Message = "Wallet not found" });
        }

        if (responseCode == "00") // Thanh toán thành công
        {
            wallet.Balance += payment.Amount;
            wallet.UpdatedAt = DateTimeOffset.UtcNow;

            walletTxn.Status = TransactionStatus.Completed;
            walletTxn.CompletedAt = DateTimeOffset.UtcNow;
            walletTxn.UpdatedAt = DateTimeOffset.UtcNow;

            payment.Status = (int)PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);

            _logger.LogInformation("[WalletIPN] Nạp tiền thành công. WalletId={WalletId}, Amount={Amount}",
                wallet.Id, payment.Amount);
        }
        else // Thất bại
        {
            walletTxn.Status = TransactionStatus.Failed;
            walletTxn.UpdatedAt = DateTimeOffset.UtcNow;

            payment.Status = (int)PaymentStatus.Failed;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);

            _logger.LogWarning("[WalletIPN] Nạp tiền thất bại. WalletId={WalletId}, Code={Code}",
                wallet.Id, responseCode);
        }

        await _paymentRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(new VnPayIpnResponse { RspCode = "00", Message = "Confirm Success" });
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
