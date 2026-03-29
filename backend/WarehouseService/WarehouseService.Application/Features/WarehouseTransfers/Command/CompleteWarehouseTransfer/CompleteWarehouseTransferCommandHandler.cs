using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.CompleteWarehouseTransfer;

public class CompleteWarehouseTransferCommandHandler(
    IWarehouseTransferRepository warehouseTransferRepository,
    IWarehouseTransferItemRepository warehouseTransferItemRepository,
    IWarehouseItemRepository warehouseItemRepository,
    IStockTransactionRepository stockTransactionRepository
) : IRequestHandler<CompleteWarehouseTransferCommand, CompleteWarehouseTransferResult>
{
    public async Task<CompleteWarehouseTransferResult> Handle(CompleteWarehouseTransferCommand request, CancellationToken cancellationToken)
    {
        // 1. Load transfer
        var transfer = await warehouseTransferRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transfer is null)
            throw new KeyNotFoundException($"Warehouse transfer '{request.Id}' was not found.");

        if (transfer.Status != TransferStatus.Approved)
            throw new InvalidOperationException($"Only approved transfers can be completed. Current status: {transfer.Status}.");

        // 2. Load only the specific items saved when transfer was created
        var transferItems = await warehouseTransferItemRepository
            .GetQueryable()
            .Where(ti => ti.WarehouseTransferId == request.Id)
            .ToListAsync(cancellationToken);

        if (!transferItems.Any())
            throw new InvalidOperationException($"Transfer '{request.Id}' has no items to process.");

        var now = DateTimeOffset.UtcNow;
        var transferredItems = new List<TransferredItemInfoDto>();

        foreach (var transferItem in transferItems)
        {
            // 3. Get source item
            var fromItem = await warehouseItemRepository.GetByIdAsync(transferItem.WarehouseItemId, cancellationToken);
            if (fromItem is null)
                throw new KeyNotFoundException($"Source item '{transferItem.WarehouseItemId}' was not found.");

            // Re-check stock at complete time
            if (fromItem.Quantity < transferItem.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for ingredient '{transferItem.IngredientId}'. " +
                    $"Available: {fromItem.Quantity}, Required: {transferItem.Quantity}.");

            // 4. Find matching destination item by IngredientId
            var toItem = await warehouseItemRepository
                .GetQueryable()
                .FirstOrDefaultAsync(
                    wi => wi.WarehouseId == transfer.ToWarehouseId
                       && wi.IngredientId == transferItem.IngredientId,
                    cancellationToken);

            if (toItem is null)
                throw new KeyNotFoundException(
                    $"Destination warehouse '{transfer.ToWarehouseId}' has no item for ingredient '{transferItem.IngredientId}'.");

            // 5. Deduct ONLY the specified quantity from source
            Console.WriteLine("--------" + transferItem.Quantity);
            fromItem.Quantity -= transferItem.Quantity;
            fromItem.UpdatedAt = now;

            // 6. Add to destination
            toItem.Quantity += transferItem.Quantity;
            toItem.UpdatedAt = now;

            // 7. TransferOut transaction
            await stockTransactionRepository.AddAsync(new StockTransaction
            {
                Id = Guid.NewGuid(),
                TransactionCode = $"TRFOUT-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                WarehouseId = transfer.FromWarehouseId,
                WarehouseItemId = fromItem.Id,
                Type = TransactionType.TransferOut,
                Quantity = transferItem.Quantity,
                UnitPrice = transferItem.UnitPrice,
                TotalPrice = transferItem.Quantity * transferItem.UnitPrice,
                WarehouseTransferId = transfer.Id,
                Status = TransactionStatus.Completed,
                CreatedBy = request.CompletedBy,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);

            // 8. TransferIn transaction
            await stockTransactionRepository.AddAsync(new StockTransaction
            {
                Id = Guid.NewGuid(),
                TransactionCode = $"TRFIN-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                WarehouseId = transfer.ToWarehouseId,
                WarehouseItemId = toItem.Id,
                Type = TransactionType.TransferIn,
                Quantity = transferItem.Quantity,
                UnitPrice = transferItem.UnitPrice,
                TotalPrice = transferItem.Quantity * transferItem.UnitPrice,
                WarehouseTransferId = transfer.Id,
                Status = TransactionStatus.Completed,
                CreatedBy = request.CompletedBy,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);

            transferredItems.Add(new TransferredItemInfoDto(
                transferItem.IngredientId,
                fromItem.Unit,
                transferItem.Quantity,
                fromItem.Id,
                toItem.Id
            ));
        }

        // 9. Mark transfer completed
        transfer.Status = TransferStatus.Completed;
        transfer.UpdatedAt = now;

        // 10. Save all in one round-trip
        await stockTransactionRepository.SaveChangesAsync(cancellationToken);

        return new CompleteWarehouseTransferResult(
            transfer.Id,
            transfer.TransferCode,
            transfer.Status,
            transfer.FromWarehouseId,
            transfer.ToWarehouseId,
            transferredItems,
            transfer.UpdatedAt
        );
    }
}