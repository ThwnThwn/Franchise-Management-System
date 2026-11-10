using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.WarehouseTransfers.Query.GetAll;

public class GetWarehouseTransfersQueryHandler(
    IWarehouseTransferRepository warehouseTransferRepository
) : IRequestHandler<GetWarehouseTransfersQuery, GetWarehouseTransfersResult>
{
    public async Task<GetWarehouseTransfersResult> Handle(GetWarehouseTransfersQuery request, CancellationToken cancellationToken)
    {
        // 1. Build base query
        var query = warehouseTransferRepository.GetQueryable();

        // 2. Apply filters
        if (request.FromWarehouseId.HasValue)
            query = query.Where(t => t.FromWarehouseId == request.FromWarehouseId.Value);

        if (request.ToWarehouseId.HasValue)
            query = query.Where(t => t.ToWarehouseId == request.ToWarehouseId.Value);

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        if (request.FromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(t => t.CreatedAt <= request.ToDate.Value);

        // 3. Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // 4. Apply pagination + ordering (newest first)
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new WarehouseTransferDto(
                t.Id,
                t.TransferCode,
                t.FromWarehouseId,
                t.ToWarehouseId,
                t.Status,
                t.Reason,
                t.CreatedBy,
                t.ApprovedBy,
                t.ApprovedAt,
                t.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        // 5. Return paginated result
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new GetWarehouseTransfersResult(
            items,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );
    }
}