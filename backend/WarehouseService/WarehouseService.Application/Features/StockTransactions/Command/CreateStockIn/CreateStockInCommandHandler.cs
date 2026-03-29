using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Command.CreateStockIn;

public class CreateStockInCommandHandler(
    IWarehouseRepository warehouseRepository,
    IWarehouseItemRepository warehouseItemRepository,
    IStockTransactionRepository stockTransactionRepository
) : IRequestHandler<CreateStockInCommand, CreateStockInResult>
{
    public async Task<CreateStockInResult> Handle(CreateStockInCommand request, CancellationToken cancellationToken)
    {
        // 1. Check warehouse exists
        var warehouse = await warehouseRepository.GetByIdAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
            throw new KeyNotFoundException($"Warehouse '{request.WarehouseId}' was not found.");

        // 2. Lookup WarehouseItem by IngredientId + WarehouseId, validate no duplicates
        var warehouseItems = new Dictionary<Guid, WarehouseItem>(); // key = IngredientId

        foreach (var stockItem in request.Items)
        {
            if (warehouseItems.ContainsKey(stockItem.IngredientId))
                throw new InvalidOperationException($"Duplicate ingredient ID '{stockItem.IngredientId}' in request.");

            var item = await warehouseItemRepository
                .GetQueryable()
                .FirstOrDefaultAsync(
                    wi => wi.WarehouseId == request.WarehouseId
                       && wi.IngredientId == stockItem.IngredientId,
                    cancellationToken);

            if (item is null)
                throw new KeyNotFoundException(
                    $"Ingredient '{stockItem.IngredientId}' does not exist in warehouse '{request.WarehouseId}'. " +
                    $"Please add the ingredient to the warehouse first.");

            warehouseItems[stockItem.IngredientId] = item;
        }

        // 3. Create transactions with Status = Pending (quantity NOT updated yet)
        var now = DateTimeOffset.UtcNow;
        var transactions = new List<StockTransaction>();

        foreach (var stockItem in request.Items)
        {
            var item = warehouseItems[stockItem.IngredientId];

            var transaction = new StockTransaction
            {
                Id = Guid.NewGuid(),
                TransactionCode = $"SI-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                WarehouseId = request.WarehouseId,
                WarehouseItemId = item.Id,
                Type = TransactionType.StockIn,
                Quantity = stockItem.Quantity,
                UnitPrice = stockItem.UnitPrice,
                TotalPrice = stockItem.Quantity * stockItem.UnitPrice,
                SupplierId = request.SupplierId,
                ReferenceCode = request.ReferenceCode,
                Note = request.Note,
                Status = TransactionStatus.Pending,
                CreatedBy = request.CreatedBy,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            await stockTransactionRepository.AddAsync(transaction, cancellationToken);
            transactions.Add(transaction);
        }

        // 4. Save all in one round-trip
        await stockTransactionRepository.SaveChangesAsync(cancellationToken);

        return new CreateStockInResult(
            transactions.Select(t => new StockInTransactionDto(
                t.Id,
                t.TransactionCode,
                t.WarehouseItemId,
                warehouseItems.First(w => w.Value.Id == t.WarehouseItemId).Key, // IngredientId
                t.Type,
                t.Quantity,
                t.UnitPrice,
                t.TotalPrice,
                t.Status
            )).ToList(),
            request.WarehouseId,
            request.SupplierId,
            request.ReferenceCode,
            transactions.Count,
            transactions.Sum(t => t.TotalPrice),
            now
        );
    }
}