using Ardalis.Result;
using MediatR;
using SalesService.Application.Authorization;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.WalletTransactions.Commands.CreateTransaction.CreateTransactionCommand;

namespace SalesService.Application.Features.WalletTransactions.Commands.CreateTransaction;

public sealed class CreateTransactionCommandHandler
    : IRequestHandler<CreateTransactionCommand, Result<CreateTransactionResponse>>
{
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IRefundAuthorizationService _authorizationService;
    private readonly IIdentityServiceClient _identityServiceClient;

    public CreateTransactionCommandHandler(
        IWalletTransactionRepository transactionRepository,
        IWalletRepository walletRepository,
        IRefundAuthorizationService authorizationService,
        IIdentityServiceClient identityServiceClient)
    {
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
        _authorizationService = authorizationService;
        _identityServiceClient = identityServiceClient;
    }

    public async Task<Result<CreateTransactionResponse>> Handle(
        CreateTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var adminOverride = command.AdminOverride;

        // ===== VALIDATE WALLET =====
        var wallet = await _walletRepository.GetByIdAsync(
            request.WalletId,
            cancellationToken);

        if (wallet == null)
            return Result.NotFound("Wallet not found");

        // Check ownership (admin can bypass)
        if (wallet.UserId != command.UserId && adminOverride == null)
            return Result.Forbidden("You don't have permission to access this wallet");

        // Check wallet status
        if (wallet.Status != WalletStatus.Active)
            return Result.Invalid(new ValidationError(
                nameof(request.WalletId),
                "Wallet is not active"));

        // ===== CHECK FOR DUPLICATE (Idempotency) =====
        if (!string.IsNullOrWhiteSpace(request.ExternalTransactionId))
        {
            var existingTransaction = await _transactionRepository
                .GetByExternalTransactionIdAsync(
                    request.ExternalTransactionId,
                    cancellationToken);

            if (existingTransaction != null)
            {
                // Return existing transaction (idempotent)
                return Result.Success(MapToResponse(existingTransaction));
            }
        }

        // ===== CALCULATE BALANCE CHANGES =====
        var balanceBefore = wallet.Balance;
        var balanceAfter = CalculateBalanceAfter(
            balanceBefore,
            request.Amount,
            request.TransactionType);

        // ===== VALIDATE BALANCE (unless bypassed) =====
        if (adminOverride == null || !adminOverride.BypassBalanceCheck)
        {
            var balanceValidation = ValidateBalance(
                request.TransactionType,
                balanceBefore,
                balanceAfter,
                request.Amount);

            if (!balanceValidation.IsSuccess)
                return balanceValidation;
        }

        // ===== VALIDATE TRANSACTION LIMITS (unless bypassed) =====
        if (adminOverride == null || !adminOverride.BypassTransactionLimits)
        {
            var limitValidation = await ValidateTransactionLimitsAsync(
                wallet.Id,
                request.Amount,
                request.TransactionType,
                cancellationToken);

            if (!limitValidation.IsSuccess)
                return limitValidation;
        }

        // ===== TYPE-SPECIFIC VALIDATIONS =====
        var typeValidation = await ValidateByTypeAsync(
            request,
            cancellationToken);

        if (!typeValidation.IsSuccess)
            return typeValidation;

        // ===== GENERATE TRANSACTION CODE =====
        var transactionCode = GenerateTransactionCode(request.TransactionType);

        // ===== DETERMINE INITIAL STATUS =====
        var initialStatus = DetermineInitialStatus(
            request.TransactionType,
            adminOverride?.AutoComplete ?? false);

        // ===== CREATE TRANSACTION ENTITY =====
        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = request.WalletId,
            UserId = command.UserId,
            TransactionCode = transactionCode,
            Amount = request.Amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            TransactionType = request.TransactionType,
            Status = initialStatus,
            PaymentMethod = request.PaymentMethod,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            OrderId = request.ReferenceType == WalletReferenceType.Order
                ? request.ReferenceId
                : null,
            Description = request.Description,
            Notes = request.Notes,
            ExternalTransactionId = request.ExternalTransactionId,
            Metadata = request.Metadata,
            InitiatedBy = command.UserId,
            ProcessedBy = adminOverride?.AdminId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // If auto-completed, set completion timestamp
        if (initialStatus == TransactionStatus.Completed)
        {
            transaction.CompletedAt = DateTimeOffset.UtcNow;
        }
        if (wallet == null)
        {
            throw new Exception("wallet is null");
        }
        else if (wallet.RowVersion == null)
        {
            throw new Exception("RowVersion is null");
        }
        // ===== UPDATE WALLET BALANCE (if completed immediately) =====
        if (initialStatus == TransactionStatus.Completed)
        {
            var balanceUpdated = await _walletRepository.UpdateBalanceAsync(
                wallet.Id,
                balanceAfter,
                wallet.RowVersion,
                cancellationToken);

            if (!balanceUpdated)
                return Result.Error(
                    "Failed to update wallet balance. " +
                    "Transaction may have been modified. Please retry.");
        }

        // ===== SAVE TRANSACTION =====
        var transactionId = await _transactionRepository.AddAsync(
            transaction,
            cancellationToken);

        if (transactionId == Guid.Empty)
            return Result.Error("Failed to create transaction");

        // ===== HANDLE TRANSFER (if applicable) =====
        if (request.TransactionType == TransactionType.Transfer &&
            request.DestinationWalletId.HasValue)
        {
            var transferResult = await CreateDestinationTransactionAsync(
                request.DestinationWalletId.Value,
                request.Amount,
                transaction.Id,
                request.Description,
                cancellationToken);

            if (!transferResult.IsSuccess)
            {
                // Rollback source transaction
                await _transactionRepository.FailTransactionAsync(
                    transactionId,
                    "Failed to create destination transaction",
                    cancellationToken);

                return Result.Error("Transfer failed: " + transferResult.Errors.FirstOrDefault());
            }
        }

        // ===== RETURN RESPONSE =====
        var response = MapToResponse(transaction);
        return Result.Success(response);
    }

    // ===== PRIVATE HELPER METHODS =====

    private static decimal CalculateBalanceAfter(
        decimal balanceBefore,
        decimal amount,
        TransactionType type)
    {
        return type switch
        {
            TransactionType.Deposit => balanceBefore + amount,
            TransactionType.Refund => balanceBefore + amount,
            TransactionType.Payment => balanceBefore - amount,
            TransactionType.Withdrawal => balanceBefore - amount,
            TransactionType.Transfer => balanceBefore - amount,
            _ => balanceBefore
        };
    }

    private static Result ValidateBalance(
        TransactionType type,
        decimal balanceBefore,
        decimal balanceAfter,
        decimal amount)
    {
        // For debit transactions, check sufficient balance
        if (type is TransactionType.Payment or
                    TransactionType.Withdrawal or
                    TransactionType.Transfer)
        {
            if (balanceAfter < 0)
                return Result.Invalid(new ValidationError(
                    "Balance",
                    $"Insufficient balance. Current: {balanceBefore:N2}, " +
                    $"Required: {amount:N2}"));
        }

        return Result.Success();
    }

    private async Task<Result> ValidateTransactionLimitsAsync(
        Guid walletId,
        decimal amount,
        TransactionType type,
        CancellationToken cancellationToken)
    {
        // Daily limit check
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todayTotal = await _transactionRepository.GetTotalAmountAsync(
            walletId,
            type,
            today,
            tomorrow,
            cancellationToken);

        const decimal DailyLimit = 100_000_000; // 100 million VND
        if (todayTotal + amount > DailyLimit)
            return Result.Invalid(new ValidationError(
                "DailyLimit",
                $"Daily transaction limit exceeded. " +
                $"Limit: {DailyLimit:N0}, Used: {todayTotal:N0}, " +
                $"Requested: {amount:N0}"));

        // Per-transaction limit
        const decimal PerTransactionLimit = 50_000_000; // 50 million VND
        if (amount > PerTransactionLimit)
            return Result.Invalid(new ValidationError(
                "TransactionLimit",
                $"Transaction amount exceeds limit of {PerTransactionLimit:N0}"));

        return Result.Success();
    }

    private async Task<Result> ValidateByTypeAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        // Payment: Validate order exists and unpaid
        if (request.TransactionType == TransactionType.Payment &&
            request.ReferenceId.HasValue)
        {
            var orderPaid = await _transactionRepository.IsOrderPaidAsync(
                request.ReferenceId.Value,
                cancellationToken);

            if (orderPaid)
                return Result.Invalid(new ValidationError(
                    "OrderId",
                    "Order has already been paid"));
        }

        // Transfer: Validate destination wallet
        if (request.TransactionType == TransactionType.Transfer &&
            request.DestinationWalletId.HasValue)
        {
            var destWallet = await _walletRepository.GetByIdAsync(
                request.DestinationWalletId.Value,
                cancellationToken);

            if (destWallet == null)
                return Result.NotFound("Destination wallet not found");

            if (destWallet.Status != WalletStatus.Active)
                return Result.Invalid(new ValidationError(
                    "DestinationWallet",
                    "Destination wallet is not active"));
        }

        return Result.Success();
    }

    private static TransactionStatus DetermineInitialStatus(
        TransactionType type,
        bool autoComplete)
    {
        // Deposits and refunds usually need confirmation
        if (type is TransactionType.Deposit)
            return autoComplete ? TransactionStatus.Completed : TransactionStatus.Pending;

        // Payments and withdrawals can be auto-completed
        if (type is TransactionType.Payment or TransactionType.Withdrawal)
            return TransactionStatus.Completed;

        // Refunds usually pending until processed
        if (type is TransactionType.Refund)
            return autoComplete ? TransactionStatus.Completed : TransactionStatus.Pending;

        return TransactionStatus.Pending;
    }

    private static string GenerateTransactionCode(TransactionType type)
    {
        var prefix = type switch
        {
            TransactionType.Deposit => "DEP",
            TransactionType.Withdrawal => "WDR",
            TransactionType.Payment => "PAY",
            TransactionType.Refund => "REF",
            TransactionType.Transfer => "TRF",
            _ => "TXN"
        };

        var shortGuid = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"{prefix}-{timestamp}-{shortGuid}";
    }

    private async Task<Result> CreateDestinationTransactionAsync(
        Guid destinationWalletId,
        decimal amount,
        Guid sourceTransactionId,
        string? description,
        CancellationToken cancellationToken)
    {
        var destWallet = await _walletRepository.GetForUpdateAsync(
            destinationWalletId,
            cancellationToken);

        if (destWallet == null)
            return Result.NotFound("Destination wallet not found");

        var destTransaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = destinationWalletId,
            UserId = destWallet.UserId,
            TransactionCode = GenerateTransactionCode(TransactionType.Deposit),
            Amount = amount,
            BalanceBefore = destWallet.Balance,
            BalanceAfter = destWallet.Balance + amount,
            TransactionType = TransactionType.Deposit,
            Status = TransactionStatus.Completed,
            Description = $"Transfer from transaction {sourceTransactionId}: {description}",
            ParentTransactionId = sourceTransactionId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };
        if(destWallet.RowVersion == null)
        {
            throw new Exception("destWallet.RowVersion is null |CreateTransaction");
        }
        // Update destination wallet balance
        var updated = await _walletRepository.UpdateBalanceAsync(
            destWallet.Id,
            destWallet.Balance + amount,
            destWallet.RowVersion,
            cancellationToken);

        if (!updated)
            return Result.Error("Failed to update destination wallet balance");

        await _transactionRepository.AddAsync(destTransaction, cancellationToken);

        return Result.Success();
    }

    private static CreateTransactionResponse MapToResponse(WalletTransaction transaction)
    {
        return new CreateTransactionResponse
        {
            Id = transaction.Id,
            TransactionCode = transaction.TransactionCode,
            WalletId = transaction.WalletId ?? Guid.Empty,
            Amount = transaction.Amount,
            TransactionType = transaction.TransactionType,
            Status = transaction.Status,
            BalanceBefore = transaction.BalanceBefore ?? 0,
            BalanceAfter = transaction.BalanceAfter ?? 0,
            ReferenceId = transaction.ReferenceId,
            ReferenceType = transaction.ReferenceType,
            PaymentMethod = transaction.PaymentMethod,
            CreatedAt = transaction.CreatedAt
        };
    }
}