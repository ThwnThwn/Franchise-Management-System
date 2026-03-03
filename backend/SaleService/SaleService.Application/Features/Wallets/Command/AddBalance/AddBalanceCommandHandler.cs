using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.Wallets.Command.AddBalance.AddBalanceCommand;

namespace SalesService.Application.Features.Wallets.Command.AddBalance;

public sealed class AddBalanceCommandHandler
    : IRequestHandler<AddBalanceCommand, Result<AddBalanceCommandResponse>>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;

    public AddBalanceCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<AddBalanceCommandResponse>> Handle(
        AddBalanceCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        if (request.Amount <= 0)
        {
            return Result.Invalid(new ValidationError(
                nameof(request.Amount),
                "Amount must be greater than zero"));
        }

        var wallet = await _walletRepository.GetForUpdateAsync(
            command.WalletId,
            cancellationToken);

        if (wallet == null)
        {
            return Result.NotFound($"Wallet {command.WalletId} not found");
        }

        if (wallet.UserId != command.UserId)
        {
            return Result.Forbidden("You don't have permission to access this wallet");
        }

        if (wallet.Status != WalletStatus.Active)
        {
            return Result.Invalid(new ValidationError(
                "WalletStatus",
                $"Wallet is not active. Current status: {wallet.Status}"));
        }

        if (!string.IsNullOrWhiteSpace(request.ExternalTransactionId))
        {
            var normalizedId = request.ExternalTransactionId.Trim().ToLower();

            var invalidValues = new[] { "string", "test", "dummy", "sample", "example" };

            if (invalidValues.Contains(normalizedId))
            {
                return Result.Invalid(new ValidationError(
                    nameof(request.ExternalTransactionId),
                    "ExternalTransactionId cannot be a generic test value. Please provide a unique transaction ID or leave it empty."));
            }

            var existingTxn = await _transactionRepository
                .GetByExternalTransactionIdAsync(
                    request.ExternalTransactionId,
                    cancellationToken);

            if (existingTxn != null)
            {
                if (existingTxn.WalletId != command.WalletId)
                {
                    return Result.Invalid(new ValidationError(
                        nameof(request.ExternalTransactionId),
                        $"ExternalTransactionId '{request.ExternalTransactionId}' already exists for a different wallet."));
                }

                // Idempotent return
                return Result.Success(new AddBalanceCommandResponse
                {
                    WalletId = existingTxn.WalletId,
                    UserId = existingTxn.UserId,
                    BalanceBefore = existingTxn.BalanceBefore ?? 0,
                    BalanceAfter = existingTxn.BalanceAfter ?? 0,
                    AmountAdded = existingTxn.Amount,
                    TransactionId = existingTxn.Id,
                    UpdatedAt = existingTxn.UpdatedAt,
                    NewRowVersion = wallet.RowVersion
                });
            }
        }

        var balanceBefore = wallet.Balance;
        var balanceAfter = balanceBefore + request.Amount;

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            UserId = wallet.UserId,
            TransactionCode = GenerateTransactionCode(),
            Amount = request.Amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            TransactionType = TransactionType.Deposit,
            Status = TransactionStatus.Completed,
            PaymentMethod =request.PaymentMethod,
            Description = request.Description ?? "Add balance to wallet",
            ExternalTransactionId = request.ExternalTransactionId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };

        await _transactionRepository.AddAsync(transaction, cancellationToken);

        try
        {
            var updated = await _walletRepository.UpdateBalanceAsync(
                wallet.Id,
                balanceAfter,
                request.RowVersion, 
                cancellationToken);

            if (!updated)
            {
                return Result.Error("Failed to update wallet balance");
            }

            var updatedWallet = await _walletRepository.GetByIdAsync(
                wallet.Id,
                cancellationToken);

            var response = new AddBalanceCommandResponse
            {
                WalletId = updatedWallet!.Id,
                UserId = updatedWallet.UserId,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                AmountAdded = request.Amount,
                TransactionId = transaction.Id, 
                UpdatedAt = updatedWallet.UpdatedAt,
                NewRowVersion = updatedWallet.RowVersion 
            };

            return Result.Success(response);
        }
        catch (DbUpdateConcurrencyException)
        {
            await _transactionRepository.FailTransactionAsync(
                transaction.Id,
                "Concurrency conflict during balance update",
                cancellationToken);

            return Result.Error(
                "Wallet was modified by another transaction. Please retry with updated RowVersion.");
        }
        catch (Exception ex)
        {
            await _transactionRepository.FailTransactionAsync(
                transaction.Id,
                $"Error: {ex.Message}",
                cancellationToken);

            return Result.Error($"Failed to add balance: {ex.Message}");
        }
    }

    private static string GenerateTransactionCode()
    {
        var shortGuid = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"DEP-{timestamp}-{shortGuid}";
    }
}