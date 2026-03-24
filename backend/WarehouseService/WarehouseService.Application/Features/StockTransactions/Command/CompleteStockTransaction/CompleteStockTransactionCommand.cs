using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Command.CompleteStockTransaction;


public sealed record CompleteStockTransactionCommand(
    IReadOnlyList<Guid> TransactionIds,
    Guid CompletedBy
) : IRequest<CompleteStockTransactionResult>;

public sealed record CompleteStockTransactionResult(
    IReadOnlyList<CompletedTransactionDto> Transactions,
    int TotalCompleted,
    decimal TotalAmount,
    DateTimeOffset CompletedAt
);

public sealed record CompletedTransactionDto(
    Guid Id,
    string TransactionCode,
    Guid WarehouseItemId,
    TransactionType Type,
    decimal Quantity,
    decimal TotalPrice,
    TransactionStatus Status
);