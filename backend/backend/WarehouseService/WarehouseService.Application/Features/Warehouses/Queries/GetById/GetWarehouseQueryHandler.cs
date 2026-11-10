using MediatR;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.Warehouses.Queries.GetById;

public class GetWarehouseQueryHandler(
    IWarehouseRepository warehouseRepository
) : IRequestHandler<GetWarehouseQuery, GetWarehouseResult>
{
    public async Task<GetWarehouseResult> Handle(GetWarehouseQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse is null)
            throw new KeyNotFoundException($"Warehouse with ID '{request.Id}' was not found.");

        return new GetWarehouseResult(
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            warehouse.Address,
            warehouse.StoreId,
            warehouse.Status,
            warehouse.CreatedAt,
            warehouse.UpdatedAt
        );
    }
}