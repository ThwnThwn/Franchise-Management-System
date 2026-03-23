using MediatR;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.WarehouseItems.Command.DeleteWarehouseItem;

public class DeleteWarehouseItemCommandHandler : IRequestHandler<DeleteWarehouseItemCommand, DeleteWarehouseItemResult>
{
    private readonly IWarehouseItemRepository _warehouseItemRepository;

    public DeleteWarehouseItemCommandHandler(IWarehouseItemRepository warehouseItemRepository)
    {
        _warehouseItemRepository = warehouseItemRepository;
    }

    public async Task<DeleteWarehouseItemResult> Handle(DeleteWarehouseItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _warehouseItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
            throw new KeyNotFoundException($"Warehouse item with ID '{request.Id}' was not found.");

        if (item.Quantity > 0)
            throw new InvalidOperationException("Warehouse item cannot be deleted because it still has stock remaining.");

        item.IsDeleted = true;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _warehouseItemRepository.SaveChangesAsync(cancellationToken);

        return new DeleteWarehouseItemResult(item.Id, item.IsDeleted);
    }
}