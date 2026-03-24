using MediatR;

namespace WarehouseService.Application.Features.Warehouses.Command.DeleteWarehouse;

public sealed record DeleteWarehouseCommand(Guid Id) : IRequest<DeleteWarehouseResult>;

public sealed record DeleteWarehouseResult(Guid Id, bool IsDeleted);