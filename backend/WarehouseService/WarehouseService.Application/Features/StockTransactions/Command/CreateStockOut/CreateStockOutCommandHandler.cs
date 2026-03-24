using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Command.CreateStockOut;

public class CreateStockOutCommandHandler : IRequestHandler<CreateStockOutCommand, CreateStockOutResult>
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IWarehouseItemRepository _warehouseItemRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;

    public CreateStockOutCommandHandler(
        IWarehouseRepository warehouseRepository,
        IWarehouseItemRepository warehouseItemRepository,
        IStockTransactionRepository stockTransactionRepository)
    {
        _warehouseRepository = warehouseRepository;
        _warehouseItemRepository = warehouseItemRepository;
        _stockTransactionRepository = stockTransactionRepository;
    }

    public async Task<CreateStockOutResult> Handle(CreateStockOutCommand request, CancellationToken cancellationToken)
    {
        // 1. Check warehouse exists
        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
            throw new KeyNotFoundException($"Warehouse '{request.WarehouseId}' was not found.");

        // 2. Lookup WarehouseItem by IngredientId, validate stock
        var warehouseItems = new Dictionary<Guid, WarehouseItem>(); // key = IngredientId

        foreach (var stockItem in request.Items)
        {
            if (warehouseItems.ContainsKey(stockItem.IngredientId))
                throw new InvalidOperationException($"Duplicate ingredient ID '{stockItem.IngredientId}' in request.");

            var item = await _warehouseItemRepository
                .GetQueryable()
                .FirstOrDefaultAsync(
                    wi => wi.WarehouseId == request.WarehouseId
                       && wi.IngredientId == stockItem.IngredientId,
                    cancellationToken);

            if (item is null)
                throw new KeyNotFoundException(
                    $"Ingredient '{stockItem.IngredientId}' does not exist in warehouse '{request.WarehouseId}'.");

            if (item.Quantity < stockItem.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for ingredient '{stockItem.IngredientId}'. " +
                    $"Available: {item.Quantity}, Requested: {stockItem.Quantity}.");

            warehouseItems[stockItem.IngredientId] = item;
        }

        // 3. Create transactions with Status = Pending (quantity NOT deducted yet)
        var now = DateTimeOffset.UtcNow;
        var transactions = new List<StockTransaction>();

        foreach (var stockItem in request.Items)
        {
            var item = warehouseItems[stockItem.IngredientId];

            var transaction = new StockTransaction
            {
                Id = Guid.NewGuid(),
                TransactionCode = $"SO-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                WarehouseId = request.WarehouseId,
                WarehouseItemId = item.Id,
                Type = TransactionType.StockOut,
                Quantity = stockItem.Quantity,
                UnitPrice = stockItem.UnitPrice,
                TotalPrice = stockItem.Quantity * stockItem.UnitPrice,
                Note = request.Note,
                Status = TransactionStatus.Pending,
                CreatedBy = request.CreatedBy,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _stockTransactionRepository.AddAsync(transaction, cancellationToken);
            transactions.Add(transaction);
        }

        // 4. Save all in one round-trip
        await _stockTransactionRepository.SaveChangesAsync(cancellationToken);

        return new CreateStockOutResult(
            transactions.Select(t => new StockOutTransactionDto(
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
            transactions.Count,
            transactions.Sum(t => t.TotalPrice),
            now
        );
    }
}