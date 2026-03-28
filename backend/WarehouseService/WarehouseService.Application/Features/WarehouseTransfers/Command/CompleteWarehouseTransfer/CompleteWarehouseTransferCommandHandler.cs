using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.CompleteWarehouseTransfer;

public class CompleteWarehouseTransferCommandHandler : IRequestHandler<CompleteWarehouseTransferCommand, CompleteWarehouseTransferResult>
{
    private readonly IWarehouseTransferRepository _warehouseTransferRepository;
    private readonly IWarehouseTransferItemRepository _warehouseTransferItemRepository;
    private readonly IWarehouseItemRepository _warehouseItemRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;

    public CompleteWarehouseTransferCommandHandler(
        IWarehouseTransferRepository warehouseTransferRepository,
        IWarehouseTransferItemRepository warehouseTransferItemRepository,
        IWarehouseItemRepository warehouseItemRepository,
        IStockTransactionRepository stockTransactionRepository)
    {
        _warehouseTransferRepository = warehouseTransferRepository;
        _warehouseTransferItemRepository = warehouseTransferItemRepository;
        _warehouseItemRepository = warehouseItemRepository;
        _stockTransactionRepository = stockTransactionRepository;
    }

    public async Task<CompleteWarehouseTransferResult> Handle(CompleteWarehouseTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _warehouseTransferRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transfer is null)
            throw new KeyNotFoundException($"Warehouse transfer '{request.Id}' was not found.");

        if (transfer.Status != TransferStatus.Approved)
            throw new InvalidOperationException($"Only approved transfers can be completed. Current status: {transfer.Status}.");

        var transferItems = await _warehouseTransferItemRepository
            .GetQueryable()
            .Where(ti => ti.WarehouseTransferId == request.Id)
            .ToListAsync(cancellationToken);

        if (transferItems.Count == 0)
            throw new InvalidOperationException($"Transfer '{request.Id}' has no items to process.");

        var now = DateTimeOffset.UtcNow;
        var transferredItems = new List<TransferredItemInfoDto>();

        foreach (var transferItem in transferItems)
        {
            var fromItem = await _warehouseItemRepository.GetByIdAsync(transferItem.WarehouseItemId, cancellationToken);
            if (fromItem is null)
                throw new KeyNotFoundException($"Source item '{transferItem.WarehouseItemId}' was not found.");

            if (fromItem.Quantity < transferItem.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for ingredient '{transferItem.IngredientId}'. " +
                    $"Available: {fromItem.Quantity}, Required: {transferItem.Quantity}.");

            var toItem = await _warehouseItemRepository
                .GetQueryable()
                .FirstOrDefaultAsync(
                    wi => wi.WarehouseId == transfer.ToWarehouseId
                       && wi.IngredientId == transferItem.IngredientId,
                    cancellationToken);

            if (toItem is null)
                throw new KeyNotFoundException(
                    $"Destination warehouse '{transfer.ToWarehouseId}' has no item for ingredient '{transferItem.IngredientId}'.");

            Console.WriteLine("--------" + transferItem.Quantity);
            fromItem.Quantity -= transferItem.Quantity;
            fromItem.UpdatedAt = now;

            toItem.Quantity += transferItem.Quantity;
            toItem.UpdatedAt = now;

            await _stockTransactionRepository.AddAsync(new StockTransaction
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

            await _stockTransactionRepository.AddAsync(new StockTransaction
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

        transfer.Status = TransferStatus.Completed;
        transfer.UpdatedAt = now;

        await _stockTransactionRepository.SaveChangesAsync(cancellationToken);

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