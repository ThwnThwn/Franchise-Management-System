using Ardalis.Result;
using MediatR;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Domain.Enums;
using SalesService.Domain.Entities;
using static SalesService.Application.Features.Wallets.Command.Create.CreateWalletCommand;

namespace SalesService.Application.Features.Wallets.Command.Create;

public sealed class CreateWalletCommandHandler
    : IRequestHandler<CreateWalletCommand, Result<CreateWalletResponse>>
{
    private readonly IWalletRepository _walletRepository;

    public CreateWalletCommandHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result<CreateWalletResponse>> Handle(
        CreateWalletCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Result.Invalid(new ValidationError(
                nameof(request.UserId),
                "UserId cannot be empty"));
        }

        var exists = await _walletRepository
            .ExistsByUserIdAsync(request.UserId, cancellationToken);

        if (exists)
        {
            return Result.Error(
                $"Wallet already exists for user {request.UserId}");
        }

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Balance = 0, 
            Status = WalletStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow, 
            UpdatedAt = DateTimeOffset.UtcNow, 
            IsDeleted = false
        };

        var walletId = await _walletRepository.AddAsync(wallet, cancellationToken);

        if (walletId == Guid.Empty)
        {
            return Result.Error("Failed to create wallet");
        }

        var response = new CreateWalletResponse
        {
            WalletId = wallet.Id,
            UserId = wallet.UserId,
            Balance = wallet.Balance,
            Status = wallet.Status,
            CreatedAt = wallet.CreatedAt
        };

        return Result.Success(response);
    }
}