using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Query.GetById;

public sealed record GetWarehouseTransferQuery(Guid Id) : IRequest<GetWarehouseTransferResult>;

public sealed record GetWarehouseTransferResult(
    Guid Id,
    string TransferCode,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    TransferStatus Status,
    string? Reason,
    string? Note,
    Guid CreatedBy,
    Guid? ApprovedBy,
    DateTime? ApprovedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<TransferItemInfoDto>? Items
);

public sealed record TransferItemInfoDto(
    Guid WarehouseItemId,
    Guid IngredientId,
    string Unit,
    decimal Quantity,
    decimal UnitPrice
);