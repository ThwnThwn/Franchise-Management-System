using MediatR;
using WarehouseService.Application.ClientInterfaces;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;

namespace WarehouseService.Application.Features.WarehouseItems.Command.CreateWarehouseItem;

public class CreateWarehouseItemCommandHandler(
    IWarehouseRepository warehouseRepository,
    IWarehouseItemRepository warehouseItemRepository,
    IInventoryServiceClient inventoryServiceClient
) : IRequestHandler<CreateWarehouseItemCommand, CreateWarehouseItemResult>
{
    public async Task<CreateWarehouseItemResult> Handle(CreateWarehouseItemCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
            throw new KeyNotFoundException($"Warehouse with ID '{request.WarehouseId}' was not found.");

        var product = await inventoryServiceClient.IngredientExistsAsync(request.IngredientId);
        if (product is false)
            throw new KeyNotFoundException($"Ingredient with ID '{request.IngredientId}' does not exist.");

        var productExists = await warehouseItemRepository.IngredientExistsInWarehouseAsync(
            request.WarehouseId, request.IngredientId, cancellationToken: cancellationToken);
        if (productExists)
            throw new InvalidOperationException($"Ingredient with ID '{request.IngredientId}' already exists in this warehouse.");

        var parsedExpiryDate = request.ExpiryDate.HasValue
            ? DateTime.SpecifyKind(request.ExpiryDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        var item = new WarehouseItem
        {
            Id = Guid.NewGuid(),
            WarehouseId = request.WarehouseId,
            IngredientId = request.IngredientId,
            Unit = request.Unit.Trim(),
            Quantity = request.Quantity,
            BasePrice = request.BasePrice,
            ExpiryDate = parsedExpiryDate, // Truyền biến đã xử lý vào đây
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await warehouseItemRepository.AddAsync(item, cancellationToken);
        await warehouseItemRepository.SaveChangesAsync(cancellationToken);

        return new CreateWarehouseItemResult(
            item.Id,
            item.WarehouseId,
            item.IngredientId,
            item.Unit,
            item.Quantity,
            item.BasePrice,
            item.ExpiryDate,
            item.CreatedAt
        );
    }
}