using System.Text.Json;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Identity;
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

public sealed class VnPayIpnHandler(
    IPaymentRepository paymentRepository,
    IWalletRepository walletRepository,
    IWalletTransactionRepository walletTransactionRepository,
    IIdentityServiceClient identityServiceClient,
    IConfiguration config,
    IOutboxRepository outboxRepository,
    IOutboxSignal outboxSignal,
    ILogger<VnPayIpnHandler> logger
) : IRequestHandler<VnPayIpnCommand, Result<VnPayIpnResponse>>
{
    public async Task<Result<VnPayIpnResponse>> Handle(
        VnPayIpnCommand request,
        CancellationToken cancellationToken)
    {
        var vnpSection = config.GetSection("VNPay");
        var hashSecret = vnpSection["HashSecret"];
        if (string.IsNullOrWhiteSpace(hashSecret))
        {
            return Result.Error("VNPay HashSecret is not configured");
        }

        var query = request.QueryParams;
        if (!query.TryGetValue("vnp_SecureHash", out var vnpSecureHash))
        {
            return Result.Success(new VnPayIpnResponse { RspCode = "97", Message = "Invalid signature" });
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

        var payment = await paymentRepository.GetByIdWithOrderAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            logger.LogWarning("[IPN] Payment {PaymentId} not found", paymentId);
            throw new Exception("payment is null |VnpayHandler");
        }

        // 1. Nhánh Nạp tiền vào ví
        if (payment.ReferenceType == PaymentReferenceType.Deposit && payment.ReferenceId.HasValue)
        {
            return await HandleWalletDepositIpnAsync(payment, responseCode, transactionNo, query, cancellationToken);
        }

        if (payment.Status == (int)PaymentStatus.Paid || payment.Status == (int)PaymentStatus.Failed)
        {
            return Result.Success(new VnPayIpnResponse { RspCode = "02", Message = "Order already confirmed" });
        }

        if (payment.ReferenceType != PaymentReferenceType.Deposit && payment.OrderId.HasValue)
        {
            payment = await paymentRepository.GetByIdWithOrderAsync(paymentId, cancellationToken);
        }

        if (long.TryParse(amountStr, out var vnpAmount))
        {
            var expected = (long)(payment!.Amount * 100);
            if (expected != vnpAmount)
            {
                await AddPaymentEvent(payment.Id, "VNPAY_IPN_AMOUNT_MISMATCH", query, cancellationToken);
                payment.Status = (int)PaymentStatus.Failed;
                payment.Provider = "vnpay";
                payment.ProviderPayload = JsonSerializer.Serialize(query);
                await paymentRepository.SaveChangesAsync(cancellationToken);

                return Result.Success(new VnPayIpnResponse { RspCode = "04", Message = "Invalid amount" });
            }
        }
        else
        {
            return Result.Success(new VnPayIpnResponse { RspCode = "04", Message = "Invalid amount" });
        }

        // 2. Nhánh Thanh toán Order
        if (responseCode == "00")
        {
            payment.Status = (int)PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);

            if (payment.Order != null)
            {
                var order = payment.Order;
                order.Status = OrderStatus.ConfirmationPending;
                order.PaidAt = DateTimeOffset.UtcNow;
                order.PaidAmount = payment.Amount;
                order.PaymentMethod = PaymentMethod.VNPay;

                // --- LOGIC TRỪ VÍ / ĐIỂM / VOUCHER BÊ TỪ RETURN SANG ---
                if (order.OrderSaleItems != null && order.OrderSaleItems.Count != 0)
                {
                    // 2.1 Trừ Voucher (Dùng Outbox thay vì Publish trực tiếp)
                    var voucherList = order.OrderSaleItems
                        .Where(x => x.Type == OrderSaleItemType.Voucher && x.ReferenceId != null)
                        .Select(x => x.ReferenceId!.Value)
                        .ToList();

                    if (voucherList.Count != 0)
                    {
                        var userVoucherEvent = new UseVouchersEvent
                        {
                            OrderId = order.Id,
                            UserId = order.CustomerId ?? Guid.Empty,
                            VoucherIds = voucherList
                        };
                        outboxRepository.AddEvent(userVoucherEvent, KafkaTopics.DeductUserVoucher);
                    }

                    // 2.2 Trừ Điểm (Dùng Outbox)
                    decimal pointAmount = order.OrderSaleItems
                        .Where(x => x.Type == OrderSaleItemType.LoyaltyPoint)
                        .Select(x => x.Amount)
                        .FirstOrDefault();

                    if (pointAmount > 0 && order.CustomerId != null)
                    {
                        MembershipTier? membershipTier = null;
                        try
                        {
                            var user = await identityServiceClient.GetUserByIdAsync(
                            order.CustomerId ?? Guid.Empty);

                            membershipTier = user?.LoyaltyTier?.CurrentTier;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to get user info for user {UserId}", order.CustomerId);
                        }

                        var usedPoint = pointAmount / (membershipTier?.ValuePerPoint ?? 1000);

                        var userPointEvent = new UpdatePointEvent
                        {
                            Id = order.CustomerId ?? Guid.Empty,
                            OrderId = order.Id,
                            Point = usedPoint,
                            ActionType = UpdatePointType.Minus
                        };
                        outboxRepository.AddEvent(userPointEvent, KafkaTopics.UpdateUserPoint);
                    }

                    // 2.3 Trừ Số dư Ví nếu có thanh toán kết hợp
                    decimal walletAmount = order.OrderSaleItems
                        .Where(x => x.Type == OrderSaleItemType.Wallet)
                        .Select(x => x.Amount)
                        .FirstOrDefault();

                    if (walletAmount > 0 && order.CustomerId != null)
                    {
                        var userWallet = await walletRepository.GetByUserIdAsync(order.CustomerId.Value, cancellationToken);
                        if (userWallet != null && userWallet.RowVersion != null)
                        {
                            var walletTransaction = new WalletTransaction
                            {
                                Id = Guid.NewGuid(),
                                TransactionCode = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
                                UserId = order.CustomerId.Value,
                                OrderId = order.Id,
                                Amount = -walletAmount,
                                TransactionType = TransactionType.Payment,
                                Status = TransactionStatus.Completed,
                                ReferenceId = order.Id,
                                ReferenceType = WalletReferenceType.Order,
                                Description = $"Sử dụng số dư ví cho đơn hàng {order.Code}",
                                PaymentMethod = order.PaymentMethod,
                                BalanceBefore = userWallet.Balance,
                                BalanceAfter = userWallet.Balance - walletAmount,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                WalletId = userWallet.Id
                            };

                            walletTransactionRepository.Add(walletTransaction);
                            walletRepository.ApplyDeductBalance(userWallet, walletAmount, userWallet.RowVersion);
                        }
                    }

                    // 2.4 Bắn Event Order
                    outboxRepository.AddEvent(OrderToKafka.MapToOrderStatusEvent(order), KafkaTopics.OrderNotifications);
                    outboxRepository.AddEvent(OrderToKafka.MapToOrderPaidEvent(order), KafkaTopics.OrderPaid);
                }
            }

            await AddPaymentEvent(payment.Id, "VNPAY_IPN_SUCCESS", query, cancellationToken);
        }
        else // Thất bại
        {
            payment.Status = (int)PaymentStatus.Failed;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);

            if (payment.Order != null)
            {
                var cancelledEvent = new OrderCancelledEvent
                {
                    OrderId = payment.Order.Id,
                    CancelledAt = DateTimeOffset.UtcNow,
                    Reason = $"Payment Failed via IPN: {responseCode}"
                };
                outboxRepository.AddEvent(cancelledEvent, KafkaTopics.OrderCancelled);
            }

            await AddPaymentEvent(payment.Id, "VNPAY_IPN_FAILED", query, cancellationToken);
        }

        // Lưu TẤT CẢ (Payment, Order, Wallet, Outbox) trong 1 Transaction
        await paymentRepository.SaveChangesAsync(cancellationToken);
        outboxSignal.Pulse();

        return Result.Success(new VnPayIpnResponse { RspCode = "00", Message = "Confirm Success" });
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
        var walletTxn = await walletTransactionRepository
            .GetByIdAsync(payment.ReferenceId!.Value, cancellationToken);

        if (walletTxn == null)
        {
            logger.LogError("[WalletIPN] WalletTransaction not found for PaymentId {PaymentId}", payment.Id);
            return Result.Success(new VnPayIpnResponse { RspCode = "01", Message = "Wallet transaction not found" });
        }

        var wallet = await walletRepository.GetByIdAsync(walletTxn.WalletId!.Value, cancellationToken);
        if (wallet == null)
        {
            logger.LogError("[WalletIPN] Wallet not found for WalletId {WalletId}", walletTxn.WalletId);
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

            logger.LogInformation("[WalletIPN] Nạp tiền thành công. WalletId={WalletId}, Amount={Amount}",
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

            logger.LogWarning("[WalletIPN] Nạp tiền thất bại. WalletId={WalletId}, Code={Code}",
                wallet.Id, responseCode);
        }

        await paymentRepository.SaveChangesAsync(cancellationToken);

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

        await paymentRepository.AddEventAsync(evt, cancellationToken);
    }
}