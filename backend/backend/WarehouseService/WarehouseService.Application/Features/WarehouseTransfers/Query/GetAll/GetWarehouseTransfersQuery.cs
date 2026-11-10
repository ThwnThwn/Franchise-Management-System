using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Query.GetAll;

public sealed record GetWarehouseTransfersQuery(
    Guid? FromWarehouseId,
    Guid? ToWarehouseId,
    TransferStatus? Status,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int Page = 1,
    int PageSize = 10
) : IRequest<GetWarehouseTransfersResult>;

public sealed record GetWarehouseTransfersResult(
    IReadOnlyList<WarehouseTransferDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public sealed record WarehouseTransferDto(
    Guid Id,
    string TransferCode,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    TransferStatus Status,
    string? Reason,
    Guid CreatedBy,
    Guid? ApprovedBy,
    DateTime? ApprovedAt,
    DateTimeOffset CreatedAt
);