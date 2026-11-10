using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Command.CreateStockIn;

public sealed record CreateStockInCommand(
    Guid WarehouseId,
    IReadOnlyList<StockInItemDto> Items,
    Guid? SupplierId,
    string? ReferenceCode,  // PurchaseOrder.Code
    string? Note,
    Guid CreatedBy
) : IRequest<CreateStockInResult>;

public sealed record StockInItemDto(
    Guid IngredientId,   
    decimal Quantity,
    decimal UnitPrice
);

public sealed record CreateStockInResult(
    IReadOnlyList<StockInTransactionDto> Transactions,
    Guid WarehouseId,
    Guid? SupplierId,
    string? ReferenceCode,
    int TotalItems,
    decimal TotalAmount,
    DateTimeOffset CreatedAt
);

public sealed record StockInTransactionDto(
    Guid Id,
    string TransactionCode,
    Guid WarehouseItemId,
    Guid IngredientId,
    TransactionType Type,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    TransactionStatus Status
);