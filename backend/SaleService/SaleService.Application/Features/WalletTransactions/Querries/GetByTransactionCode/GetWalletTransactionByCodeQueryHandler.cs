using Ardalis.Result;
using MediatR;
using SalesService.Application.Features.WalletTransactions.Dtos;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.WalletTransactions;

namespace SalesService.Application.Features.WalletTransactions.Queries.GetByCode;

public sealed class GetWalletTransactionByCodeQueryHandler
    : IRequestHandler<GetWalletTransactionByCodeQuery, Result<WalletTransactionResponse>>
{
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IIdentityServiceClient _identityServiceClient;

    public GetWalletTransactionByCodeQueryHandler(
        IWalletTransactionRepository walletTransactionRepository, IIdentityServiceClient identityServiceClient)
    {
        _walletTransactionRepository = walletTransactionRepository;
        _identityServiceClient = identityServiceClient;
    }

    public async Task<Result<WalletTransactionResponse>> Handle(
        GetWalletTransactionByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var transaction = await _walletTransactionRepository
            .GetByTransactionCodeAsync(request.TransactionCode, cancellationToken);

        if (transaction == null)
            return Result.NotFound("Wallet transaction not found.");

        var userRole = await _identityServiceClient.GetUserByIdAsync(request.LoggedUserId);
        Console.WriteLine($"User Role Slug: {userRole?.Role?.Slug}");
        var isOwner = transaction.UserId == request.LoggedUserId;
        var slug = userRole?.Role?.Slug?.Trim().ToLower();

        var isInternalUser = slug == "admin"
                        || slug == "manager"
                        || slug == "staff";

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