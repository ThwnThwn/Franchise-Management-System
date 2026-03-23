using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.WarehouseItems.Query.GetWarehouseItems;

public class GetWarehouseItemsQueryHandler : IRequestHandler<GetWarehouseItemsQuery, GetWarehouseItemsResult>
{
    private readonly IWarehouseItemRepository _warehouseItemRepository;

    public GetWarehouseItemsQueryHandler(IWarehouseItemRepository warehouseItemRepository)
    {
        _warehouseItemRepository = warehouseItemRepository;
    }

    public async Task<GetWarehouseItemsResult> Handle(GetWarehouseItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _warehouseItemRepository.GetQueryable()
            .Where(wi => wi.WarehouseId == request.WarehouseId);

        //  Apply optional filters
        if (request.IngredientId.HasValue)
            query = query.Where(wi => wi.IngredientId == request.IngredientId.Value);

        if (request.LowStock == true)
            query = query.Where(wi => wi.Quantity <= 10);

        if (request.ExpiringSoon == true)
        {
            var threshold = DateTime.UtcNow.AddDays(30);
            query = query.Where(wi => wi.ExpiryDate.HasValue && wi.ExpiryDate.Value <= threshold);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(wi => wi.ExpiryDate)
            .ThenBy(wi => wi.Quantity)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(wi => new WarehouseItemDto(
                wi.Id,
                wi.WarehouseId,
                wi.IngredientId,
                wi.Unit,
                wi.Quantity,
                wi.BasePrice,
                wi.ExpiryDate,
                wi.CreatedAt,
                wi.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new GetWarehouseItemsResult(
            items,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );
    }
}