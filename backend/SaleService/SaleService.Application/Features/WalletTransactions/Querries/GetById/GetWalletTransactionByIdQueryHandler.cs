using Ardalis.Result;
using MediatR;
using SalesService.Application.Features.WalletTransactions.Dtos;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.WalletTransactions;

namespace SalesService.Application.Features.WalletTransactions.Queries.GetById;

public sealed class GetWalletTransactionByIdQueryHandler
    : IRequestHandler<GetWalletTransactionByIdQuery, Result<WalletTransactionResponse>>
{
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IIdentityServiceClient _identityServiceClient;

    public GetWalletTransactionByIdQueryHandler(
        IWalletTransactionRepository transactionRepository, IIdentityServiceClient identityServiceClien)
    {
        _transactionRepository = transactionRepository;
        _identityServiceClient = identityServiceClien;
    }

    public async Task<Result<WalletTransactionResponse>> Handle(
        GetWalletTransactionByIdQuery query,
        CancellationToken cancellationToken)
    {
        var userRole = await _identityServiceClient.GetUserByIdAsync(query.LoggedUserId);

        var transaction = await _transactionRepository.GetByIdAsync(
            query.TransactionId,
            cancellationToken);

        if (transaction == null)
        {
            return Result.NotFound("Transaction not found");
        }

        var isOwner = query.LoggedUserId == transaction.UserId;
        var role = userRole?.Role?.Slug?.Trim().ToLower();

        var isInternalUser = role == "administrator"
                        || role == "manager"
                        || role == "staff";

        if (!isOwner && !isInternalUser)
        {
            return Result.Forbidden("You do not have permission!!!");
        }

        var response = new WalletTransactionResponse
        {
            Id = transaction.Id,
            WalletId = transaction.WalletId ?? Guid.Empty,
            UserId = transaction.UserId,

            TransactionCode = transaction.TransactionCode,
            Amount = transaction.Amount,
            TransactionType = transaction.TransactionType,
            Status = transaction.Status,

            BalanceBefore = transaction.BalanceBefore,
            BalanceAfter = transaction.BalanceAfter,

            PaymentMethod = transaction.PaymentMethod,
            Description = transaction.Description,
            Notes = transaction.Notes,

            ReferenceId = transaction.ReferenceId,
            ReferenceType = transaction.ReferenceType,
            OrderId = transaction.OrderId,

            ExternalTransactionId = transaction.ExternalTransactionId,

            FailureReason = transaction.FailureReason,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
            CompletedAt = transaction.CompletedAt,
            FailedAt = transaction.FailedAt,

            ParentTransactionId = transaction.ParentTransactionId
        };

        return Result.Success(response);
    }
}