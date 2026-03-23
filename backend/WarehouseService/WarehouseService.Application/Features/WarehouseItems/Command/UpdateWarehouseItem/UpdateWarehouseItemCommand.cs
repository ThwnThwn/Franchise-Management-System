using MediatR;

namespace WarehouseService.Application.Features.WarehouseItems.Command.UpdateWarehouseItem;

public sealed record UpdateWarehouseItemCommand(
    Guid Id,
    string Unit,
    decimal BasePrice,
    DateTime? ExpiryDate
) : IRequest<UpdateWarehouseItemResult>;

public sealed record UpdateWarehouseItemResult(
    Guid Id,
    Guid WarehouseId,
    Guid IngredientID,
    string Unit,
    decimal Quantity,
    decimal BasePrice,
    DateTime? ExpiryDate,
    DateTimeOffset UpdatedAt
);