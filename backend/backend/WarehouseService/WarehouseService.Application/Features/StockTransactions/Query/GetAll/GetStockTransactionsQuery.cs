using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Query.GetAll;

public sealed record GetStockTransactionsQuery(
    Guid? WarehouseId = null,
    Guid? WarehouseItemId = null,
    TransactionType? Type = null,
    TransactionStatus? Status = null,
    Guid? SupplierId = null,
    string? ReferenceCode = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
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