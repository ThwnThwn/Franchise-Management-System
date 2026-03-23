using MediatR;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Command.CompleteStockTransaction;

public class CompleteStockTransactionCommandHandler : IRequestHandler<CompleteStockTransactionCommand, CompleteStockTransactionResult>
{
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly IWarehouseItemRepository _warehouseItemRepository;

    public CompleteStockTransactionCommandHandler(
        IStockTransactionRepository stockTransactionRepository,
        IWarehouseItemRepository warehouseItemRepository)
    {
        _stockTransactionRepository = stockTransactionRepository;
        _warehouseItemRepository = warehouseItemRepository;
    }

    public async Task<CompleteStockTransactionResult> Handle(CompleteStockTransactionCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var completed = new List<CompletedTransactionDto>();

        foreach (var transactionId in request.TransactionIds)
        {
            var transaction = await _stockTransactionRepository.GetByIdAsync(transactionId, cancellationToken);
            if (transaction is null)
                throw new KeyNotFoundException($"Stock transaction '{transactionId}' was not found.");

            //  Only StockIn/StockOut + Pending can be completed
            if (transaction.Type != TransactionType.StockIn && transaction.Type != TransactionType.StockOut)
                throw new InvalidOperationException($"Transaction '{transactionId}' is type '{transaction.Type}' — only StockIn/StockOut can be completed.");

            if (transaction.Status != TransactionStatus.Pending)
                throw new InvalidOperationException($"Transaction '{transactionId}' is not Pending. Current status: {transaction.Status}.");

            //  Get warehouse item
            var item = await _warehouseItemRepository.GetByIdAsync(transaction.WarehouseItemId, cancellationToken);
            if (item is null)
                throw new KeyNotFoundException($"Warehouse item '{transaction.WarehouseItemId}' was not found.");

            //  Update quantity based on type
            if (transaction.Type == TransactionType.StockIn)
            {
                item.Quantity += transaction.Quantity;
            }
            else // StockOut
            {
                if (item.Quantity < transaction.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for item '{transaction.WarehouseItemId}'. " +
                        $"Available: {item.Quantity}, Requested: {transaction.Quantity}.");

                item.Quantity -= transaction.Quantity;
            }

            item.UpdatedAt = now;

            transaction.Status = TransactionStatus.Completed;
            transaction.UpdatedAt = now;

            completed.Add(new CompletedTransactionDto(
                transaction.Id,
                transaction.TransactionCode,
                transaction.WarehouseItemId,
                transaction.Type,
                transaction.Quantity,
                transaction.TotalPrice,
                transaction.Status
            ));
        }

        await _stockTransactionRepository.SaveChangesAsync(cancellationToken);

        return new CompleteStockTransactionResult(
            completed,
            completed.Count,
            completed.Sum(t => t.TotalPrice),
            now
        );
    }
}