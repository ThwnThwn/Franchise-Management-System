using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.CompleteWarehouseTransfer;

public sealed record CompleteWarehouseTransferCommand(
    Guid Id,
    Guid CompletedBy
) : IRequest<CompleteWarehouseTransferResult>;

public sealed record CompleteWarehouseTransferResult(
    Guid Id,
    string TransferCode,
    TransferStatus Status,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    IReadOnlyList<TransferredItemInfoDto> TransferredItems,
    DateTimeOffset UpdatedAt
);

public sealed record TransferredItemInfoDto(
    Guid IngredientId,
    string Unit,
    decimal Quantity,
    Guid FromWarehouseItemId,
    Guid ToWarehouseItemId
);