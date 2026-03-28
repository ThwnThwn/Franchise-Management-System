using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.Warehouses.Queries.GetAll;

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, GetWarehousesResult>
{
    private readonly IWarehouseRepository _warehouseRepository;

    public GetWarehousesQueryHandler(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<GetWarehousesResult> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var query = _warehouseRepository.GetQueryable();

        if (request.StoreId.HasValue)
            query = query.Where(w => w.StoreId == request.StoreId.Value);

        if (request.Status.HasValue)
            query = query.Where(w => w.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
#pragma warning disable CA2249 // EF Core LINQ-to-SQL cannot translate StringComparison overload
            query = query.Where(w =>
                w.Code.ToLower().Contains(term) ||
                w.Name.ToLower().Contains(term) ||
                w.Address.ToLower().Contains(term));
#pragma warning restore CA2249
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(w => w.Code)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WarehouseDto(
                w.Id,
                w.Code,
                w.Name,
                w.Address,
                w.StoreId,
                w.Status,
                w.CreatedAt,
                w.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new GetWarehousesResult(
            items,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );
    }
}