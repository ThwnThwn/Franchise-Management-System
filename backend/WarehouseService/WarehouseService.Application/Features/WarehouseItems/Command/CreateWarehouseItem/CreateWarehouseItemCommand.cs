using MediatR;

namespace WarehouseService.Application.Features.WarehouseItems.Command.CreateWarehouseItem;

public sealed record CreateWarehouseItemCommand(
    Guid WarehouseId,
    Guid IngredientId,
    string Unit,
    decimal Quantity,
    decimal BasePrice,
    DateTime? ExpiryDate
) : IRequest<CreateWarehouseItemResult>;

public sealed record CreateWarehouseItemResult(
    Guid Id,
    Guid WarehouseId,
    Guid IngredientId,
    string Unit,
    decimal Quantity,
    decimal BasePrice,
    DateTime? ExpiryDate,
    DateTimeOffset CreatedAt
);