using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Wallets.Deposit;

public sealed class DepositWalletHandler
    : IRequestHandler<DepositWalletCommand, Result<DepositWalletRes>>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IConfiguration _config;

    public DepositWalletHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IPaymentRepository paymentRepository,
        IConfiguration config)
    {
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _paymentRepository = paymentRepository;
        _config = config;
    }

    public async Task<Result<DepositWalletRes>> Handle(
        DepositWalletCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // ===== STEP 1: VALIDATE AMOUNT =====
        if (request.Amount <= 0)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(request.Amount),
                ErrorMessage = "Deposit amount must be greater than zero"
            });
        }

        // ===== STEP 2: GET OR CREATE WALLET =====
        var wallet = await _walletRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (wallet == null)
        {
            return Result.NotFound("Wallet not found");
        }

        // ===== STEP 3: CREATE PENDING WALLET TRANSACTION =====
        // ✅ Chưa cộng tiền — chờ VNPay callback xác nhận
        var walletTransaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            WalletId = wallet.Id,
            TransactionCode = $"DEP-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Amount = request.Amount,
            TransactionType = TransactionType.Deposit,
            PaymentMethod = PaymentMethod.VNPay,
            Status = TransactionStatus.Pending, 
            ReferenceType = WalletReferenceType.Deposit,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _walletTransactionRepository.AddAsync(walletTransaction, cancellationToken);

        // ===== STEP 4: CREATE PAYMENT (dùng để track VNPay) =====
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Code = $"PAY-DEP-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Amount = request.Amount,
            Currency = "VND",
            Method = (int)PaymentMethod.VNPay,
            Status = (int)PaymentStatus.Pending,
            ReferenceId = walletTransaction.Id,         
            ReferenceType = PaymentReferenceType.Deposit,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _paymentRepository.CreateAsync(payment, cancellationToken);

        // ===== STEP 5: BUILD VNPAY URL =====
        var vnpSection = _config.GetSection("VNPay");

        var callbackUrl = vnpSection["WalletVNPayReturn"]!;
        if (!string.IsNullOrEmpty(request.ReturnUrl))
            callbackUrl += $"?redirectTo={Uri.EscapeDataString(request.ReturnUrl)}";

        var vnpParams = new Dictionary<string, string>
        {
            ["vnp_Version"] = vnpSection["Version"] ?? "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = vnpSection["TmnCode"]!,
            ["vnp_Amount"] = ((long)(request.Amount * 100)).ToString(),
            ["vnp_CreateDate"] = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = "127.0.0.1",
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = $"Nap tien vao vi {command.UserId}",
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = callbackUrl,
            ["vnp_TxnRef"] = payment.Id.ToString() 
        };

        var paymentUrl = VNPayHelper.BuildPaymentUrl(
            vnpSection["PaymentUrl"]!,
            vnpSection["HashSecret"]!,
            vnpParams
        );
        return Result.Success(new DepositWalletRes { PaymentUrl = paymentUrl });
    }
}