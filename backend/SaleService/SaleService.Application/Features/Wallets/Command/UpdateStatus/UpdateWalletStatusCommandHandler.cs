using Ardalis.Result;
using MediatR;
using SalesService.Application.Features.Wallets.Commands.UpdateStatus;
using SalesService.Application.Persisitences.Wallets;
using static SalesService.Application.Features.Wallets.Command.DeductBalance.DeductBalanceCommand;
using static SalesService.Application.Features.Wallets.Commands.UpdateStatus.UpdateWalletStatusCommand;

namespace SalesService.Application.Features.Wallets.Command.UpdateStatus;

public sealed class UpdateWalletStatusCommandHandler :
    IRequestHandler<UpdateWalletStatusCommand, Result<UpdateWalletStatusResponse>>
{
    private readonly IWalletRepository _walletRepository;
    public UpdateWalletStatusCommandHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result<UpdateWalletStatusResponse>> Handle(
        UpdateWalletStatusCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var wallet = await _walletRepository.GetByIdAsync(
            command.WalletId,
            cancellationToken);

        if (wallet == null)
        {
            return Result<UpdateWalletStatusResponse>.NotFound(
                $"Wallet with id '{command.WalletId}' is not found");
        }

        if( wallet.Status == request.NewStatus)
        {
            return Result<UpdateWalletStatusResponse>.Error(
                $"Wallet with id '{command.WalletId}' is already in status '{request.NewStatus}'");
        }

        if (await _walletRepository.HasPendingIssuesAsync(command.WalletId, cancellationToken))
        {
            return Result<UpdateWalletStatusResponse>.Error(
                $"Cannot update status for wallet with id '{command.WalletId}' because it has pending issues");
        }

        try
        {
            var success =await _walletRepository.UpdateStatusAsync(
                command.WalletId,
                request.NewStatus,
                cancellationToken);

            if (!success)
            {
                return Result<UpdateWalletStatusResponse>.Error(
                    $"Failed to update status for wallet with id '{command.WalletId}'");
            }

            var updatedWallet = await _walletRepository.GetByIdAsync(
                command.WalletId,
                cancellationToken);

            var response = new UpdateWalletStatusResponse
            {
                WalletId = updatedWallet.Id,
                OldStatus = wallet.Status,
                NewStatus = updatedWallet.Status,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            return Result<UpdateWalletStatusResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<UpdateWalletStatusResponse>.Error(
                $"An error occurred while updating wallet status: {ex.Message}");
        }
    }
}
