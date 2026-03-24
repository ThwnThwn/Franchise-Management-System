using MediatR;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.CreateWarehouseTransfer;

public class CreateWarehouseTransferCommandHandler : IRequestHandler<CreateWarehouseTransferCommand, CreateWarehouseTransferResult>
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IWarehouseItemRepository _warehouseItemRepository;
    private readonly IWarehouseTransferRepository _warehouseTransferRepository;
    private readonly IWarehouseTransferItemRepository _warehouseTransferItemRepository;

    public CreateWarehouseTransferCommandHandler(
        IWarehouseRepository warehouseRepository,
        IWarehouseItemRepository warehouseItemRepository,
        IWarehouseTransferRepository warehouseTransferRepository,
        IWarehouseTransferItemRepository warehouseTransferItemRepository)
    {
        _warehouseRepository = warehouseRepository;
        _warehouseItemRepository = warehouseItemRepository;
        _warehouseTransferRepository = warehouseTransferRepository;
        _warehouseTransferItemRepository = warehouseTransferItemRepository;
    }

    public async Task<CreateWarehouseTransferResult> Handle(CreateWarehouseTransferCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate warehouses
        var fromWarehouse = await _warehouseRepository.GetByIdAsync(request.FromWarehouseId, cancellationToken);
        if (fromWarehouse is null)
            throw new KeyNotFoundException($"Source warehouse '{request.FromWarehouseId}' was not found.");

        var toWarehouse = await _warehouseRepository.GetByIdAsync(request.ToWarehouseId, cancellationToken);
        if (toWarehouse is null)
            throw new KeyNotFoundException($"Destination warehouse '{request.ToWarehouseId}' was not found.");

        if (request.FromWarehouseId == request.ToWarehouseId)
            throw new InvalidOperationException("Source and destination warehouses must be different.");

        var now = DateTimeOffset.UtcNow;
        var transferItemEntities = new List<WarehouseTransferItem>();
        var itemInfos = new List<TransferItemInfoDto>();

        // 2. Validate items + build TransferItem entities
        foreach (var transferItem in request.Items)
        {
            var item = await _warehouseItemRepository.GetByIdAsync(transferItem.WarehouseItemId, cancellationToken);
            if (item is null)
                throw new KeyNotFoundException($"Warehouse item '{transferItem.WarehouseItemId}' was not found.");

            if (item.WarehouseId != request.FromWarehouseId)
                throw new InvalidOperationException($"Item '{transferItem.WarehouseItemId}' does not belong to source warehouse.");

            if (item.Quantity < transferItem.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for item '{transferItem.WarehouseItemId}'. " +
                    $"Available: {item.Quantity}, Requested: {transferItem.Quantity}.");

            transferItemEntities.Add(new WarehouseTransferItem
            {
                Id = Guid.NewGuid(),
                WarehouseItemId = item.Id,
                IngredientId = item.IngredientId,
                Quantity = transferItem.Quantity,
                UnitPrice = item.BasePrice,
                CreatedAt = now,
                UpdatedAt = now
            });

            itemInfos.Add(new TransferItemInfoDto(
                item.Id,
                item.IngredientId,
                item.Unit,
                transferItem.Quantity,
                item.BasePrice
            ));
        }

        // 3. Create transfer header
        var transfer = new WarehouseTransfer
        {
            Id = Guid.NewGuid(),
            TransferCode = $"TRF-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            Status = TransferStatus.Pending,
            Reason = request.Reason,
            Note = request.Note,
            CreatedBy = request.CreatedBy,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        // 4. Link items to transfer before saving
        foreach (var ti in transferItemEntities)
            ti.WarehouseTransferId = transfer.Id;

        // Save transfer header + items in ONE round-trip (same DbContext = same transaction)
        await _warehouseTransferRepository.AddAsync(transfer, cancellationToken);
        await _warehouseTransferItemRepository.AddRangeAsync(transferItemEntities, cancellationToken);
        await _warehouseTransferItemRepository.SaveChangesAsync(cancellationToken);

        return new CreateWarehouseTransferResult(
            transfer.Id,
            transfer.TransferCode,
            transfer.FromWarehouseId,
            transfer.ToWarehouseId,
            transfer.Status,
            transfer.Reason,
            transfer.Note,
            transfer.CreatedBy,
            transfer.CreatedAt,
            itemInfos
        );
    }
}