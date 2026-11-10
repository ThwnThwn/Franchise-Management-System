using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.Warehouses.Queries.GetAll;

public sealed record GetWarehousesQuery(
    Guid? StoreId,
    WarehouseStatus? Status,
    string? SearchTerm, // Search by Code,Address or Name
    int Page = 1,
    int PageSize = 10
) : IRequest<GetWarehousesResult>;

public sealed record GetWarehousesResult(
    IReadOnlyList<WarehouseDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public sealed record WarehouseDto(
    Guid Id,
    string Code,
    string Name,
    string Address,
    Guid StoreId,
    WarehouseStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);