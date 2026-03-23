using MediatR;
using WarehouseService.Application.Features.WarehouseItems.Query.GetById;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.WarehouseItems.Query.GetById;

public class GetWarehouseItemQueryHandler : IRequestHandler<GetWarehouseItemQuery, GetWarehouseItemResult>
{
    private readonly IWarehouseItemRepository _warehouseItemRepository;

    public GetWarehouseItemQueryHandler(IWarehouseItemRepository warehouseItemRepository)
    {
        _warehouseItemRepository = warehouseItemRepository;
    }

    public async Task<GetWarehouseItemResult> Handle(GetWarehouseItemQuery request, CancellationToken cancellationToken)
    {
        var item = await _warehouseItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
            throw new KeyNotFoundException($"Warehouse item with ID '{request.Id}' was not found.");

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