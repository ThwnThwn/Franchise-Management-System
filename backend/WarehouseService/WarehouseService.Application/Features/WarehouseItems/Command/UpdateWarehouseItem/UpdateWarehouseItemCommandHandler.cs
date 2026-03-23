using MediatR;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.WarehouseItems.Command.UpdateWarehouseItem;

public class UpdateWarehouseItemCommandHandler : IRequestHandler<UpdateWarehouseItemCommand, UpdateWarehouseItemResult>
{
    private readonly IWarehouseItemRepository _warehouseItemRepository;

    public UpdateWarehouseItemCommandHandler(IWarehouseItemRepository warehouseItemRepository)
    {
        _warehouseItemRepository = warehouseItemRepository;
    }

    public async Task<UpdateWarehouseItemResult> Handle(UpdateWarehouseItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _warehouseItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
            throw new KeyNotFoundException($"Warehouse item with ID '{request.Id}' was not found.");

        item.Unit = request.Unit.Trim();
        item.BasePrice = request.BasePrice;
        item.ExpiryDate = request.ExpiryDate;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _warehouseItemRepository.SaveChangesAsync(cancellationToken);

        return new UpdateWarehouseItemResult(
            item.Id,
            item.WarehouseId,
            item.IngredientId,
            item.Unit,
            item.Quantity,
            item.BasePrice,
            item.ExpiryDate,
            item.UpdatedAt
        );
    }
}