using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.StockTransactions.Query.GetAll;

public class GetStockTransactionsQueryHandler(
    IStockTransactionRepository stockTransactionRepository,
    IWarehouseItemRepository warehouseItemRepository
) : IRequestHandler<GetStockTransactionsQuery, GetStockTransactionsResult>
{
    public async Task<GetStockTransactionsResult> Handle(GetStockTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = stockTransactionRepository.GetQueryable();

        if (request.WarehouseId.HasValue)
            query = query.Where(t => t.WarehouseId == request.WarehouseId.Value);

        if (request.WarehouseItemId.HasValue)
            query = query.Where(t => t.WarehouseItemId == request.WarehouseItemId.Value);

        if (request.Type.HasValue)
            query = query.Where(t => t.Type == request.Type.Value);

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        if (request.SupplierId.HasValue)
            query = query.Where(t => t.SupplierId == request.SupplierId.Value);

        if (!string.IsNullOrWhiteSpace(request.ReferenceCode))
            query = query.Where(t => t.ReferenceCode == request.ReferenceCode.Trim());

        if (request.FromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(t => t.CreatedAt <= request.ToDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var warehouseItems = warehouseItemRepository.GetQueryable();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Join(
                warehouseItems,
                t => t.WarehouseItemId,
                wi => wi.Id,
                (t, wi) => new StockTransactionDto(
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
                    t.CreatedAt
                )
            )
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new GetStockTransactionsResult(items, totalCount, request.Page, request.PageSize, totalPages);
    }
}