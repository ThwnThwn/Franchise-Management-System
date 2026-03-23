using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Command.CancelStockTransaction;

public sealed record CancelStockTransactionCommand(Guid Id, Guid UpdatedBy) : IRequest<CancelStockTransactionResult>;

public sealed record CancelStockTransactionResult(
    Guid Id,
    string TransactionCode,
    Guid WarehouseId, 
    Guid WarehouseItemId,
    TransactionType Type, 
    TransactionStatus Status, 
    DateTimeOffset UpdatedAt
);