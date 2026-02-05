using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.Wallets.Command.DeductBalance.DeductBalanceCommand;
namespace SalesService.Application.Features.Wallets.Command.DeductBalance;

public sealed class DeductBalanceCommandHandler :
    IRequestHandler<DeductBalanceCommand, Result<DeductBalanceCommandResponse>>
{
    private readonly IWalletRepository _walletRepository;
    public DeductBalanceCommandHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result<DeductBalanceCommandResponse>> Handle(
        DeductBalanceCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var wallet = await _walletRepository.GetByIdAsync(
            command.WalletId,
            cancellationToken);

        if (wallet == null)
        {
            return Result<DeductBalanceCommandResponse>.NotFound(
                $"Wallet with id '{command.WalletId}' is not found");
        }

        if (wallet.Status != WalletStatus.Active)
        {
            return Result<DeductBalanceCommandResponse>.Invalid(
                new ValidationError($"Wallet is not active. Current status: {wallet.Status}"));
        }

        try
        {
            // Step 4: Add balance
            var success = await _walletRepository.DeductBalanceAsync(
                command.WalletId,
                request.Amount,
                wallet.RowVersion,
                cancellationToken);

            if (!success)
            {
                return Result<DeductBalanceCommandResponse>.Error(
                    "Failed to add balance to wallet");
            }

            var updatedWallet = await _walletRepository.GetByIdAsync(
                command.WalletId,
                cancellationToken);

            var response = new DeductBalanceCommandResponse
            {
                WalletId = updatedWallet.Id,
                BalanceBefore = wallet.Balance,
                AmountDeducted = request.Amount,
                BalanceAfter = updatedWallet.Balance,
                UpdatedAt = updatedWallet.UpdatedAt
            };

            return Result<DeductBalanceCommandResponse>.Success(response);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<DeductBalanceCommandResponse>.Error(
                "Wallet was modified by another transaction. Please retry.");
        }
        catch (Exception ex)
        {
            return Result<DeductBalanceCommandResponse>.Error(
                $"An error occurred while adding balance: {ex.Message}");
        }
    }
}