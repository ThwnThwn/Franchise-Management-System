using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.Warehouses.Command.CreateWarehouse;

public sealed record CreateWarehouseCommand(
    string Code,
    string Name,
    string Address,
    Guid StoreId,
    WarehouseStatus Status
) : IRequest<CreateWarehouseResult>;

public sealed record CreateWarehouseResult(
    Guid Id,
    string Code,
    string Name,
    string Address,
    Guid StoreId,
    WarehouseStatus Status,
    DateTimeOffset CreatedAt
);