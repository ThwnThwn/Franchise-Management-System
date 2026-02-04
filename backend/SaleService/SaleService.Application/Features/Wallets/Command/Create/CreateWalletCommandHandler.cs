using MediatR;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Domain.Enums;
using SalesService.Domain.Entities;

namespace SalesService.Application.Features.Wallets.Command.Create
{
    public sealed class CreateWalletCommandHandler
        : IRequestHandler<CreateWalletCommand, Guid>
    {
        private readonly IWalletRepository _walletRepository;

        public CreateWalletCommandHandler(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<Guid> Handle(
            CreateWalletCommand request,
            CancellationToken cancellationToken)
        {
            var exists = await _walletRepository
                .ExistsByUserIdAsync(request.UserId, cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException(
                    $"Wallet already exists for user {request.UserId}");
            }

            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Balance = 0,
                Status = WalletStatus.Active,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _walletRepository.AddAsync(wallet, cancellationToken);

            return wallet.Id;
        }
    }
}
