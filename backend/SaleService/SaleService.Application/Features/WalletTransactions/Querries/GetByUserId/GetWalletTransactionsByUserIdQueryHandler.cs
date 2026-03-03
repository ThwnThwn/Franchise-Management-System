using Ardalis.Result;
using MediatR;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.WalletTransactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SalesService.Application.Features.WalletTransactions.Queries.GetByUserId;

public sealed class GetWalletTransactionsByUserIdQueryHandler
    : IRequestHandler<GetWalletTransactionsByUserIdQuery, Result<BuildingBlocks.Core.Pagination.PagedResult<WalletTransactionResponse>>>
{
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IIdentityServiceClient _identityServiceClient;

    public GetWalletTransactionsByUserIdQueryHandler(
        IWalletTransactionRepository walletTransactionRepository, IIdentityServiceClient identityServiceClient)
    {
        _walletTransactionRepository = walletTransactionRepository;
        _identityServiceClient = identityServiceClient;
    }

    public async Task<Result<BuildingBlocks.Core.Pagination.PagedResult<WalletTransactionResponse>>> Handle(
        GetWalletTransactionsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var transactions = await _walletTransactionRepository
            .GetByUserIdAsync(request.UserIdToSearch,request.PageIndex, request.PageSize,cancellationToken);

        if (transactions == null)
            return Result.NotFound("No wallet transactions found.");

        var userRole = await _identityServiceClient.GetUserByIdAsync(request.LoggedUserId);
        var isOwner = request.UserIdToSearch == request.LoggedUserId;
        var slug = userRole?.Role?.Slug?.Trim().ToLower();

        var isInternalUser = slug == "admin"
                        || slug == "manager"
                        || slug == "staff";

        if (!isOwner && !isInternalUser)
        {
            return Result.Forbidden("You do not have permission!!!");
        }

        var response = transactions.Items.Select(t => new WalletTransactionResponse
        {
            Id = t.Id,
            WalletId = t.WalletId,
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

        return Result.Success(new BuildingBlocks.Core.Pagination.PagedResult<WalletTransactionResponse>(
            response,
            transactions.TotalCount,
            request.PageIndex,
            request.PageSize));
    }
}
