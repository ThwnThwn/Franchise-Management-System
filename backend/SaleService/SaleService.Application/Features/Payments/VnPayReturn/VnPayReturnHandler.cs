using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace SalesService.Application.Features.Payments.VnPayReturn;

public sealed class VnPayReturnHandler(
    IPaymentRepository paymentRepository,
    IConfiguration config,
    ILogger<VnPayReturnHandler> logger,
    IKafkaProducer kafkaProducer,
    IWalletRepository walletRepository,
    IWalletTransactionRepository walletTransactionRepository
) : IRequestHandler<VnPayReturnCommand, Result<VnPayReturnResponse>>
{
    public async Task<Result<VnPayReturnResponse>> Handle(
        VnPayReturnCommand request,
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
            return Result.Invalid(
            [
                new ValidationError
                {
                    Identifier = "vnp_SecureHash",
                    ErrorMessage = "Missing vnp_SecureHash"
                }
            ]);
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
            return Result.Invalid(
            [
                new ValidationError
                {
                    Identifier = "vnp_TxnRef",
                    ErrorMessage = "Invalid vnp_TxnRef"
                }
            ]);
        }

        var payment = await paymentRepository.GetByIdWithOrderAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            return Result.NotFound("Payment not found");
        }

        var order = payment.Order;

        // Chỉ kiểm tra logic để hiển thị cho UI (KHÔNG LƯU DB, KHÔNG BẮN KAFKA)
        // Mọi việc cập nhật dữ liệu để VnPayIpnHandler lo gánh
        var isSuccess = responseCode == "00";

        if (isSuccess)
        {
            //=================
            // Kakfa Event Voucher
            //=================
            List<Guid> voucherList = order.OrderSaleItems
                .Where(x => x.Type == OrderSaleItemType.Voucher && x.ReferenceId != null)
                .Select(x => x.ReferenceId!.Value)
                .ToList();
            if (voucherList.Count != 0)
            {
                UseVouchersEvent userVoucherEvent = new()
                {
                    OrderId = order.Id,
                    UserId = order.CustomerId ?? Guid.Empty,
                    VoucherIds = voucherList
                };
                await kafkaProducer.PublishAsync(KafkaTopics.DeductUserVoucher, userVoucherEvent);
            }
            //=================
            // Kakfa Event Point
            //=================
            decimal point = order.OrderSaleItems
                .Where(x => x.Type == OrderSaleItemType.LoyaltyPoint)
                .Select(x => x.Amount)
                .FirstOrDefault();
            if (point != 0)
            {
                UpdatePointEvent userPoint = new UpdatePointEvent
                {
                    Id = order.CustomerId ?? Guid.Empty,
                    OrderId = order.Id,
                    Point = point,
                    ActionType = UpdatePointType.Minus
                };
                await kafkaProducer.PublishAsync(KafkaTopics.UpdateUserPoint, userPoint);
            }
            //=================
            // Wallet Event
            //=================
            if (order.CustomerId != null)
            {
                var userWallet = await walletRepository.GetByUserIdAsync(order.CustomerId.Value, cancellationToken);

                decimal walletAmount = order.OrderSaleItems
                    .Where(x => x.Type == OrderSaleItemType.Wallet)
                    .Select(x => x.Amount)
                    .FirstOrDefault();

                if (walletAmount != 0 && userWallet != null && userWallet.RowVersion != null)
                {
                    var walletTransaction = new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = order.CustomerId ?? Guid.Empty,
                        OrderId = order.Id,
                        Amount = walletAmount,
                        TransactionType = TransactionType.Payment,
                        Status = TransactionStatus.Completed,
                        ReferenceId = order.Id,
                        ReferenceType = WalletReferenceType.Order,
                        Description = $"Sử dụng số dư ví cho đơn hàng {order.Code}",
                        PaymentMethod = order.PaymentMethod,
                        BalanceBefore = userWallet.Balance,
                        BalanceAfter = userWallet.Balance - walletAmount,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await walletTransactionRepository.AddAsync(walletTransaction, cancellationToken);
                    await walletRepository.DeductBalanceAsync(userWallet.Id, walletAmount, userWallet.RowVersion, cancellationToken);
                }
            }

            //=================
            // log
            logger.LogInformation("VNPay Return: Giao dịch thành công cho PaymentId {PaymentId}. Chờ IPN xử lý DB.", payment.Id);
        }
        else
        {
            logger.LogWarning("VNPay Return: Giao dịch thất bại cho PaymentId {PaymentId} (Mã: {ResponseCode}).", payment.Id, responseCode);
        }

        return Result.Success(new VnPayReturnResponse
        {
            Success = isSuccess,
            PaymentId = payment.Id,
            OrderId = payment.OrderId ?? Guid.Empty,
            ResponseCode = responseCode ?? string.Empty
        });
    }
}