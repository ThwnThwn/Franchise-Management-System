using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.CreateWarehouseTransfer;

public sealed record CreateWarehouseTransferCommand(
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    IReadOnlyList<TransferItemDto> Items,
    string? Reason,
    string? Note,
    Guid CreatedBy
) : IRequest<CreateWarehouseTransferResult>;

public sealed record TransferItemDto(Guid WarehouseItemId, decimal Quantity);

public sealed record CreateWarehouseTransferResult(
    Guid Id,
    string TransferCode,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    TransferStatus Status,
    string? Reason,
    string? Note,
    Guid CreatedBy,
    DateTimeOffset CreatedAt,
    IReadOnlyList<TransferItemInfoDto> Items
);

public sealed record TransferItemInfoDto(
    Guid WarehouseItemId,
    Guid IngredientId,
    string Unit,
    decimal Quantity,
    decimal UnitPrice
);