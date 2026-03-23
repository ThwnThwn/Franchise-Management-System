using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Query.GetById;

public sealed record GetStockTransactionQuery(Guid Id) : IRequest<GetStockTransactionResult>;

public sealed record GetStockTransactionResult(
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
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);