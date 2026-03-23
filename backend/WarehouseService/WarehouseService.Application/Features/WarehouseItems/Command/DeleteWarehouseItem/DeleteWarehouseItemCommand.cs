using MediatR;

namespace WarehouseService.Application.Features.WarehouseItems.Command.DeleteWarehouseItem;

public sealed record DeleteWarehouseItemCommand(Guid Id) : IRequest<DeleteWarehouseItemResult>;

public sealed record DeleteWarehouseItemResult(Guid Id, bool IsDeleted);