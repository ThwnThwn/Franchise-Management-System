using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.Warehouses.Command.UpdateWarehouse;

public sealed record UpdateWarehouseCommand(
    Guid Id,
    string? Code,
    string? Name,
    string? Address,
    WarehouseStatus? Status,
    Guid? StoreId
) : IRequest<UpdateWarehouseResult>;

public sealed record UpdateWarehouseResult(
    Guid Id,
    string Code,
    string Name,
    string Address,
    Guid StoreId,
    WarehouseStatus Status,
    DateTimeOffset UpdatedAt
);