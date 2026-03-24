using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Query.GetAll;

public sealed record GetStockTransactionsQuery(
    Guid? WarehouseId,
    Guid? WarehouseItemId,
    TransactionType? Type,
    TransactionStatus? Status,
    Guid? SupplierId,
    string? ReferenceCode,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int Page = 1,
    int PageSize = 10
) : IRequest<GetStockTransactionsResult>;

public sealed record GetStockTransactionsResult(
    IReadOnlyList<StockTransactionDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public sealed record StockTransactionDto(
    Guid Id,
    string TransactionCode,
    Guid WarehouseId,
    Guid WarehouseItemId,
    Guid IngredientId,        
    TransactionType Type,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    Guid? SupplierId,
    string? ReferenceCode,
    Guid? WarehouseTransferId,
    string? Note,
    TransactionStatus Status,
    Guid CreatedBy,
    DateTimeOffset CreatedAt
);