using MediatR;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.Warehouses.Command.DeleteWarehouse;

public class DeleteWarehouseCommandHandler(
    IWarehouseRepository warehouseRepository
) : IRequestHandler<DeleteWarehouseCommand, DeleteWarehouseResult>
{
    public async Task<DeleteWarehouseResult> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse is null)
            throw new KeyNotFoundException($"Warehouse with ID '{request.Id}' was not found.");

        //  Check warehouse has active items before deleting
        var hasActiveItems = warehouseRepository.GetQueryable()
            .Where(w => w.Id == request.Id)
            .SelectMany(w => w.WarehouseItems)
            .Any(wi => !wi.IsDeleted && wi.Quantity > 0);

        if (hasActiveItems)
            throw new InvalidOperationException("Warehouse cannot be deleted because it still has items in stock.");

        //  Soft delete
        warehouse.IsDeleted = true;
        warehouse.UpdatedAt = DateTimeOffset.UtcNow;

        await warehouseRepository.SaveChangesAsync(cancellationToken);

        return new DeleteWarehouseResult(warehouse.Id, warehouse.IsDeleted);
    }
}