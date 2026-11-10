using Ardalis.Result;
using MediatR;
using SalesService.Application.Features.WalletTransactions.Dtos;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.WalletTransactions;

namespace SalesService.Application.Features.WalletTransactions.Querries.GetByOrderId
{
    public sealed class GetWalletTransactionByOrderIdQueryHandler :
        IRequestHandler<GetWalletTransactionByOrderIdQuery, Result<List<WalletTransactionResponse>>>
    {
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IIdentityServiceClient _identityServiceClient;

        public GetWalletTransactionByOrderIdQueryHandler(
            IWalletTransactionRepository walletTransactionRepository, IIdentityServiceClient identityServiceClient)
        {
            _walletTransactionRepository = walletTransactionRepository;
            _identityServiceClient = identityServiceClient;
        }

        public async Task<Result<List<WalletTransactionResponse>>> Handle(
        GetWalletTransactionByOrderIdQuery request,
        CancellationToken cancellationToken)
        {
            var transactions = await _walletTransactionRepository
                .GetByOrderIdAsync(request.OrderId);

            if (transactions == null || !transactions.Any())
                return Result.NotFound("No wallet transactions found.");

            var userRole = await _identityServiceClient.GetUserByIdAsync(request.LoggedUserId);
            var isOwner = transactions.First().UserId == request.LoggedUserId;
            var slug = userRole?.Role?.Slug?.Trim().ToLower();

            var isInternalUser = slug == "admin"
                            || slug == "manager"
                            || slug == "staff";

            if (!isOwner && !isInternalUser)
            {
                return Result.Forbidden("You do not have permission!!!");
            }

            var response = transactions.Select(t => new WalletTransactionResponse
            {
                Id = t.Id,
                WalletId = t.WalletId ?? Guid.Empty,
                UserId = t.UserId,
                OrderId = t.OrderId,
                InitiatedBy = t.InitiatedBy,
                ProcessedBy = t.ProcessedBy,
                ParentTransactionId = t.ParentTransactionId,
                TransactionCode = t.TransactionCode,
                Notes = t.Notes,
                Amount = t.Amount,
                BalanceBefore = t.BalanceBefore,
                BalanceAfter = t.BalanceAfter,
                TransactionType = t.TransactionType,
                Status = t.Status,
                ReferenceId = t.ReferenceId,
                ReferenceType = t.ReferenceType,
                Description = t.Description,
                ExternalTransactionId = t.ExternalTransactionId,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CompletedAt = t.CompletedAt,
                FailedAt = t.FailedAt,
                FailureReason = t.FailureReason,
                Metadata = t.Metadata,
                PaymentMethod = t.PaymentMethod,
                ProcessedAt = t.ProcessedAt
            }).ToList();

            return Result.Success(response);
        }
    }
}