using Ardalis.Result;
using MediatR;
using SalesService.Application.Persisitences.WalletEvents;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Wallets.Withdraw;

public sealed class WithdrawWalletHandler
    : IRequestHandler<WithdrawWalletCommand, Result<WithdrawWalletRes>>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IWalletEventRepository _walletEventRepository;

    public WithdrawWalletHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IWalletEventRepository walletEventRepository)
    {
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _walletEventRepository = walletEventRepository;
    }

    public async Task<Result<WithdrawWalletRes>> Handle(
        WithdrawWalletCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // ===== STEP 1: VALIDATE =====
        if (request.Amount <= 0)
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(request.Amount),
                ErrorMessage = "Withdraw amount must be greater than zero"
            });

        // ===== STEP 2: GET WALLET =====
        var wallet = await _walletRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (wallet == null)
            return Result.NotFound("Wallet not found");

        // ===== STEP 3: CHECK BALANCE =====
        if (wallet.Balance < request.Amount)
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(request.Amount),
                ErrorMessage = $"Insufficient balance. Available: {wallet.Balance:N2}"
            });

        var withdrawTransaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            WalletId = wallet.Id,
            TransactionCode = $"WDR-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Amount = -request.Amount,
            TransactionType = TransactionType.Withdrawal,
            PaymentMethod = PaymentMethod.VNPay,
            Status = TransactionStatus.Pending,
            ReferenceType = WalletReferenceType.Withdrawal,
            Notes = request.Reason,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _walletTransactionRepository.AddAsync(withdrawTransaction, cancellationToken);


        var walletEvent = new WalletEvent
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            UserId = command.UserId,
            WalletTransactionId = withdrawTransaction.Id,
            EventType = WalletEventType.Withdraw,
            Amount = request.Amount,
            Status = WalletEventStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _walletEventRepository.AddAsync(walletEvent, cancellationToken);

        return Result.Success(new WithdrawWalletRes
        {
            WithdrawTransactionId = withdrawTransaction.Id,
            Amount = request.Amount,
            Status = "Pending",
            Message = "Withdraw request is pending to be processed by Admin"
        });
    }
}
