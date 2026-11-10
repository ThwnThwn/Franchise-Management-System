using MediatR;

namespace WarehouseService.Application.Features.WarehouseItems.Query.GetAll;

public sealed record GetWarehouseItemsQuery(
    Guid WarehouseId,
    Guid? IngredientId,
    bool? LowStock,          // true = chỉ lấy hàng sắp hết (Quantity <= 10)
    bool? ExpiringSoon,      // true = chỉ lấy hàng sắp hết hạn trong 30 ngày
    int Page = 1,
    int PageSize = 10
) : IRequest<GetWarehouseItemsResult>;

public sealed record GetWarehouseItemsResult(
    IReadOnlyList<WarehouseItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public sealed record WarehouseItemDto(
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