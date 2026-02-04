using MediatR;
using Ardalis.Result;
using SalesService.Application.Persisitences.Wallets;

namespace SalesService.Application.Features.Wallets.Queries.GetByUserId;

    public sealed class GetWalletByUserIdQueryHandler :
        IRequestHandler<GetWalletByUserIdQuery, Result<GetWalletByUserIdQueryResponse>>
    {
        private readonly IWalletRepository _walletRepository;
        public GetWalletByUserIdQueryHandler(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<Result<GetWalletByUserIdQueryResponse>> Handle(
            GetWalletByUserIdQuery query,
            CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByUserIdAsync(
                query.UserId,
                cancellationToken);
            if (wallet == null)
            {
                return Result<GetWalletByUserIdQueryResponse>.NotFound(
                    $"Wallet for user id '{query.UserId}' is not found");
            }
            var response = new GetWalletByUserIdQueryResponse
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = wallet.Balance,
                RowVersion = wallet.RowVersion,
                Status = (int)wallet.Status,
                CreatedAt = wallet.CreatedAt,
                UpdatedAt = wallet.UpdatedAt
            };
            return Result<GetWalletByUserIdQueryResponse>.Success(response);
        }
    }

