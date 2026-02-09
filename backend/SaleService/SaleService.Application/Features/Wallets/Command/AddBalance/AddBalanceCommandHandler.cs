using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.Wallets.Command.AddBalance.AddBalanceCommand;

namespace SalesService.Application.Features.Wallets.Command.AddBalance;

public sealed class AddBalanceCommandHandler :
    IRequestHandler<AddBalanceCommand, Result<AddBalanceCommandResponse>>
{
    private readonly IWalletRepository _walletRepository;

    public AddBalanceCommandHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result<AddBalanceCommandResponse>> Handle(
        AddBalanceCommand command,
        CancellationToken cancellationToken)
    {

        var wallet = await _walletRepository.GetByIdAsync(
            command.WalletId,
            cancellationToken);

        if (wallet == null)
        {
            return Result<AddBalanceCommandResponse>.NotFound(
                $"Wallet with id '{command.WalletId}' is not found");
        }

        if (wallet.Status != WalletStatus.Active)
        {
            return Result<AddBalanceCommandResponse>.Invalid(
                new ValidationError($"Wallet is not active. Current status: {wallet.Status}"));
        }

        try
        {
            // Step 4: Add balance
            var success = await _walletRepository.AddBalanceAsync(
                command.WalletId,
                command.Request.Amount,
                wallet.RowVersion, 
                cancellationToken);

            if (!success)
            {
                return Result<AddBalanceCommandResponse>.Error(
                    "Failed to add balance to wallet");
            }

            var updatedWallet = await _walletRepository.GetByIdAsync(
                command.WalletId,
                cancellationToken);

            var response = new AddBalanceCommandResponse
            {
                WalletId = updatedWallet.Id,
                BalanceBefore = wallet.Balance,
                AmountAdded = command.Request.Amount,
                BalanceAfter = updatedWallet.Balance,
                UpdatedAt = updatedWallet.UpdatedAt
            };

            return Result<AddBalanceCommandResponse>.Success(response);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<AddBalanceCommandResponse>.Error(
                "Wallet was modified by another transaction. Please retry.");
        }
        catch (Exception ex)
        {
            return Result<AddBalanceCommandResponse>.Error(
                $"An error occurred while adding balance: {ex.Message}");
        }
    }
}