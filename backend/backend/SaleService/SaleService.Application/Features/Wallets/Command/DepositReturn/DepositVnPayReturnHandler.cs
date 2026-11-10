using System.Text.Json;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Wallets.Deposit;

/// <summary>
/// Xử lý Return URL của VNPay sau khi user thanh toán nạp ví.
/// Cập nhật DB trực tiếp từ Return URL (fallback nếu IPN không đến).
/// Có idempotency check: không cập nhật lại nếu IPN đã xử lý trước rồi.
/// </summary>
public sealed class DepositVnPayReturnHandler
    : IRequestHandler<DepositVnPayReturnCommand, Result<DepositVnPayReturnRes>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IConfiguration _config;

    public DepositVnPayReturnHandler(
        IPaymentRepository paymentRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IConfiguration config)
    {
        _paymentRepository = paymentRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _config = config;
    }

    public async Task<Result<DepositVnPayReturnRes>> Handle(
        DepositVnPayReturnCommand request,
        CancellationToken cancellationToken)
    {
        var query = request.QueryParams;

        // ===== STEP 1: VERIFY SIGNATURE =====
        var hashSecret = _config["VNPay:HashSecret"];
        if (!query.TryGetValue("vnp_SecureHash", out var secureHash))
            return Result.Invalid(new ValidationError
            {
                Identifier = "vnp_SecureHash",
                ErrorMessage = "Missing vnp_SecureHash"
            });

        var vnpParams = query
            .Where(kv => kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase)
                      && !kv.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                      && !kv.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        if (!VNPayHelper.VerifySignature(hashSecret!, vnpParams, secureHash))
            return Result.Error("Invalid VNPay signature");

        // ===== STEP 2: PARSE PARAMS =====
        query.TryGetValue("vnp_TxnRef", out var txnRef);
        query.TryGetValue("vnp_ResponseCode", out var responseCode);
        query.TryGetValue("vnp_TransactionNo", out var transactionNo);
        query.TryGetValue("vnp_Amount", out var amountStr);

        if (!Guid.TryParse(txnRef, out var paymentId))
            return Result.Invalid(new ValidationError
            {
                Identifier = "vnp_TxnRef",
                ErrorMessage = "Invalid vnp_TxnRef"
            });

        // ===== STEP 3: GET PAYMENT =====
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return Result.NotFound("Payment not found");

        // ===== STEP 3.5: IDEMPOTENCY — IPN đã xử lý trước rồi thì bỏ qua =====
        if (payment.Status == (int)PaymentStatus.Paid || payment.Status == (int)PaymentStatus.Failed)
        {
            var isSuccessAlready = payment.Status == (int)PaymentStatus.Paid;
            decimal currentBalance = 0;

            if (payment.ReferenceId.HasValue)
            {
                var walletTxn = await _walletTransactionRepository.GetByIdAsync(payment.ReferenceId.Value, cancellationToken);
                if (walletTxn != null)
                {
                    var walletIdem = await _walletRepository.GetByIdAsync(walletTxn.WalletId!.Value, cancellationToken);
                    if (walletIdem != null) currentBalance = walletIdem.Balance;
                }
            }

            return Result.Success(new DepositVnPayReturnRes
            {
                Success = isSuccessAlready,
                WalletTransactionId = payment.ReferenceId ?? Guid.Empty,
                Amount = payment.Amount,
                NewBalance = currentBalance
            });
        }

        // ===== STEP 4: VALIDATE AMOUNT =====
        if (long.TryParse(amountStr, out var vnpAmount))
        {
            var expected = (long)(payment.Amount * 100);
            if (expected != vnpAmount)
            {
                payment.Status = (int)PaymentStatus.Failed;
                payment.ProviderPayload = JsonSerializer.Serialize(query);
                await _paymentRepository.SaveChangesAsync(cancellationToken);
                return Result.Error("Amount mismatch");
            }
        }

        // ===== STEP 5: GET WALLET TRANSACTION =====
        var walletTransaction = await _walletTransactionRepository
            .GetByIdAsync(payment.ReferenceId!.Value, cancellationToken);

        if (walletTransaction == null)
            return Result.NotFound("Wallet transaction not found");

        // ===== STEP 6: GET WALLET =====
        var wallet = await _walletRepository
            .GetByIdAsync(walletTransaction.WalletId!.Value, cancellationToken);

        if (wallet == null)
            return Result.NotFound("Wallet not found");

        // ===== STEP 7: PROCESS RESULT =====
        if (responseCode == "00") // Thành công
        {
            wallet.Balance += payment.Amount;
            wallet.UpdatedAt = DateTimeOffset.UtcNow;

            walletTransaction.Status = TransactionStatus.Completed;
            walletTransaction.CompletedAt = DateTimeOffset.UtcNow;
            walletTransaction.UpdatedAt = DateTimeOffset.UtcNow;
            walletTransaction.BalanceBefore = wallet.Balance - payment.Amount;
            walletTransaction.BalanceAfter = wallet.Balance;

            payment.Status = (int)PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);
        }
        else // Thất bại
        {
            walletTransaction.Status = TransactionStatus.Failed;
            walletTransaction.UpdatedAt = DateTimeOffset.UtcNow;

            payment.Status = (int)PaymentStatus.Failed;
            payment.Provider = "vnpay";
            payment.ProviderTxnId = transactionNo;
            payment.ProviderPayload = JsonSerializer.Serialize(query);
        }

        await _paymentRepository.SaveChangesAsync(cancellationToken);

        var isSuccess = responseCode == "00";
        return Result.Success(new DepositVnPayReturnRes
        {
            Success = isSuccess,
            WalletTransactionId = payment.ReferenceId ?? Guid.Empty,
            Amount = payment.Amount,
            NewBalance = wallet.Balance
        });
    }
}