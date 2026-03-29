using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.StockTransactions.Query.GetById;

public class GetStockTransactionQueryHandler(
    IStockTransactionRepository stockTransactionRepository,
    IWarehouseItemRepository warehouseItemRepository
) : IRequestHandler<GetStockTransactionQuery, GetStockTransactionResult>
{
    public async Task<GetStockTransactionResult> Handle(GetStockTransactionQuery request,
        CancellationToken cancellationToken)
    {
        var warehouseItems = warehouseItemRepository.GetQueryable();

        var result = await stockTransactionRepository.GetQueryable()
            .Where(t => t.Id == request.Id)
            .Join(
                warehouseItems,
                t => t.WarehouseItemId,
                wi => wi.Id,
                (t, wi) => new GetStockTransactionResult(
                    t.Id,
                    t.TransactionCode,
                    t.WarehouseId,
                    t.WarehouseItemId,
                    wi.IngredientId,
                    t.Type,
                    t.Quantity,
                    t.UnitPrice,
                    t.TotalPrice,
                    t.SupplierId,
                    t.ReferenceCode,
                    t.WarehouseTransferId,
                    t.Note,
                    t.Status,
                    t.CreatedBy.GetValueOrDefault(),
                    t.CreatedAt,
                    t.UpdatedAt
                )
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
            throw new KeyNotFoundException($"Stock transaction with ID '{request.Id}' was not found.");

        return result;
    }
}