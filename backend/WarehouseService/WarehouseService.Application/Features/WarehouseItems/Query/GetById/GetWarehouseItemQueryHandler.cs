using MediatR;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.WarehouseItems.Query.GetById;

public class GetWarehouseItemQueryHandler(
    IWarehouseItemRepository warehouseItemRepository
) : IRequestHandler<GetWarehouseItemQuery, GetWarehouseItemResult>
{
    public async Task<GetWarehouseItemResult> Handle(GetWarehouseItemQuery request, CancellationToken cancellationToken)
    {
        var item = await warehouseItemRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Warehouse item with ID '{request.Id}' was not found.");

        return new GetWarehouseItemResult(
            item.Id,
            item.WarehouseId,
            item.IngredientId,
            item.Unit,
            item.Quantity,
            item.BasePrice,
            item.ExpiryDate,
            item.CreatedAt,
            item.UpdatedAt
        );
    }
}