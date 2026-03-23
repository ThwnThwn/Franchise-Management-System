using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Command.CreateStockOut;

public sealed record CreateStockOutCommand(
    Guid WarehouseId,
    IReadOnlyList<StockOutItemDto> Items,
    string? Note,
    Guid CreatedBy
) : IRequest<CreateStockOutResult>;

public sealed record StockOutItemDto(
    Guid IngredientId,   
    decimal Quantity,
    decimal UnitPrice
);

public sealed record CreateStockOutResult(
    IReadOnlyList<StockOutTransactionDto> Transactions,
    Guid WarehouseId,
    int TotalItems,
    decimal TotalAmount,
    DateTimeOffset CreatedAt
);

public sealed record StockOutTransactionDto(
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