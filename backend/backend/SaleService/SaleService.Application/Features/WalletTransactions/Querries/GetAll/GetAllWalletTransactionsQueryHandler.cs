using Ardalis.Result;
using MediatR;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Features.WalletTransactions.Dtos;

namespace SalesService.Application.Features.WalletTransactions.Queries.GetAll;

public sealed class GetAllWalletTransactionsQueryHandler
    : IRequestHandler<GetAllWalletTransactionsQuery, Result<BuildingBlocks.Core.Pagination.PagedResult<WalletTransactionResponse>>>
{
    private readonly IWalletTransactionRepository _walletTransactionRepository;

    public GetAllWalletTransactionsQueryHandler(
        IWalletTransactionRepository walletTransactionRepository)
    {
        _walletTransactionRepository = walletTransactionRepository;
    }

    public async Task<Result<BuildingBlocks.Core.Pagination.PagedResult<WalletTransactionResponse>>> Handle(
        GetAllWalletTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        var transactions = await _walletTransactionRepository
            .GetAllTransactionsAsync(query.PageIndex, query.PageSize, cancellationToken);

        if (transactions == null)
            return Result.NotFound("No wallet transactions found.");

        var response = transactions.Items.Select(t => new WalletTransactionResponse
        {
            Id = t.Id,
            UserId = t.UserId,
            WalletId = t.WalletId ?? Guid.Empty,
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
            query.PageIndex,
            query.PageSize));
    }
}