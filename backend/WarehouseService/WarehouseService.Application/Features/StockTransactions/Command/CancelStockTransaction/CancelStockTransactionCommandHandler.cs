using MediatR;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Command.CancelStockTransaction;

public class CancelStockTransactionCommandHandler : IRequestHandler<CancelStockTransactionCommand, CancelStockTransactionResult>
{
    private readonly IStockTransactionRepository _stockTransactionRepository;

    public CancelStockTransactionCommandHandler(IStockTransactionRepository stockTransactionRepository)
    {
        _stockTransactionRepository = stockTransactionRepository;
    }

    public async Task<CancelStockTransactionResult> Handle(CancelStockTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _stockTransactionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transaction is null)
            throw new KeyNotFoundException($"Stock transaction with ID '{request.Id}' was not found.");

        //  Only Pending transactions can be cancelled
        // Completed transactions have already updated quantity — cannot reverse here
        if (transaction.Status != TransactionStatus.Pending)
            throw new InvalidOperationException(
                $"Only pending transactions can be cancelled. Current status: {transaction.Status}.");

        if (transaction.Type == TransactionType.TransferIn || transaction.Type == TransactionType.TransferOut)
            throw new InvalidOperationException("Transfer transactions cannot be cancelled directly. Cancel the warehouse transfer instead.");

        // Pending = quantity not yet applied, so simply mark as Cancelled
        var now = DateTimeOffset.UtcNow;
        transaction.Status = TransactionStatus.Cancelled;
        transaction.UpdatedAt = now;

        await _stockTransactionRepository.SaveChangesAsync(cancellationToken);

        return new CancelStockTransactionResult(
            transaction.Id,
            transaction.TransactionCode,
            transaction.WarehouseId,
            transaction.WarehouseItemId,
            transaction.Type,
            transaction.Status,
            transaction.UpdatedAt
        );
    }
}