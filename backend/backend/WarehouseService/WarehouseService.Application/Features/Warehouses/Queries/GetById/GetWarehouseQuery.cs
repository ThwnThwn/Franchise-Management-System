using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.Warehouses.Queries.GetById;

public sealed record GetWarehouseQuery(Guid Id) : IRequest<GetWarehouseResult>;

public sealed record GetWarehouseResult(
    Guid Id,
    string Code,
    string Name,
    string Address,
    Guid StoreId,
    WarehouseStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);