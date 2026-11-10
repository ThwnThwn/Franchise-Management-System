using MediatR;

namespace WarehouseService.Application.Features.WarehouseItems.Query.GetById;

public sealed record GetWarehouseItemQuery(Guid Id) : IRequest<GetWarehouseItemResult>;

public sealed record GetWarehouseItemResult(
    Guid Id,
    Guid WarehouseId,
    Guid IngredientId,
    string Unit,
    decimal Quantity,
    decimal BasePrice,
    DateTime? ExpiryDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);