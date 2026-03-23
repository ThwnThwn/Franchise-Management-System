using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.RejectWarehouseTransfer;

public sealed record RejectWarehouseTransferCommand(Guid Id, string? Reason, Guid RejectedBy) : IRequest<RejectWarehouseTransferResult>;

public sealed record RejectWarehouseTransferResult(
    Guid Id,
    string TransferCode,
    TransferStatus Status,
    string? Reason,
    DateTimeOffset UpdatedAt
);