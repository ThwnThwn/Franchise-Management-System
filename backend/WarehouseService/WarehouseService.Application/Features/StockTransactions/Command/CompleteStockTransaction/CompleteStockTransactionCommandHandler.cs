using MediatR;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Command.CompleteStockTransaction;

public class CompleteStockTransactionCommandHandler(
    IStockTransactionRepository stockTransactionRepository,
    IWarehouseItemRepository warehouseItemRepository
) : IRequestHandler<CompleteStockTransactionCommand, CompleteStockTransactionResult>
{
    public async Task<CompleteStockTransactionResult> Handle(CompleteStockTransactionCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var completed = new List<CompletedTransactionDto>();

        foreach (var transactionId in request.TransactionIds)
        {
            var transaction = await stockTransactionRepository.GetByIdAsync(transactionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Stock transaction '{transactionId}' was not found.");

            //  Only StockIn/StockOut + Pending can be completed
            if (transaction.Type != TransactionType.StockIn && transaction.Type != TransactionType.StockOut)
                throw new InvalidOperationException($"Transaction '{transactionId}' is type '{transaction.Type}' — only StockIn/StockOut can be completed.");

            if (transaction.Status != TransactionStatus.Pending)
                throw new InvalidOperationException($"Transaction '{transactionId}' is not Pending. Current status: {transaction.Status}.");

            //  Get warehouse item
            var item = await warehouseItemRepository.GetByIdAsync(transaction.WarehouseItemId, cancellationToken)
                ?? throw new KeyNotFoundException($"Warehouse item '{transaction.WarehouseItemId}' was not found.");

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

        await stockTransactionRepository.SaveChangesAsync(cancellationToken);

        return new CompleteStockTransactionResult(
            completed,
            completed.Count,
            completed.Sum(t => t.TotalPrice),
            now
        );
    }
}