using MediatR;
using WarehouseService.Application.ClientInterfaces;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;

namespace WarehouseService.Application.Features.WarehouseItems.Command.CreateWarehouseItem;

public class CreateWarehouseItemCommandHandler : IRequestHandler<CreateWarehouseItemCommand, CreateWarehouseItemResult>
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IWarehouseItemRepository _warehouseItemRepository;
    private readonly IInventoryServiceClient _inventoryServiceClient;
    public CreateWarehouseItemCommandHandler(
        IWarehouseRepository warehouseRepository,
        IWarehouseItemRepository warehouseItemRepository,
        IInventoryServiceClient inventoryServiceClient)
    {
        _warehouseRepository = warehouseRepository;
        _warehouseItemRepository = warehouseItemRepository;
        _inventoryServiceClient=inventoryServiceClient;
    }

    public async Task<CreateWarehouseItemResult> Handle(CreateWarehouseItemCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
            throw new KeyNotFoundException($"Warehouse with ID '{request.WarehouseId}' was not found.");

        var product = await _inventoryServiceClient.IngredientExistsAsync(request.IngredientId);
        if (product is false)
            throw new KeyNotFoundException($"Ingredient with ID '{request.IngredientId}' does not exist.");

        var productExists = await _warehouseItemRepository.IngredientExistsInWarehouseAsync(
            request.WarehouseId, request.IngredientId, cancellationToken: cancellationToken);
        if (productExists)
            throw new InvalidOperationException($"Ingredient with ID '{request.IngredientId}' already exists in this warehouse.");


        var item = new WarehouseItem
        {
            Id = Guid.NewGuid(),
            WarehouseId = request.WarehouseId,
            IngredientId = request.IngredientId,
            Unit = request.Unit.Trim(),
            Quantity = request.Quantity,
            BasePrice = request.BasePrice,
            ExpiryDate = request.ExpiryDate.HasValue
            ? DateTime.SpecifyKind(request.ExpiryDate.Value, DateTimeKind.Utc)
            : null,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _warehouseItemRepository.AddAsync(item, cancellationToken);
        await _warehouseItemRepository.SaveChangesAsync(cancellationToken);

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