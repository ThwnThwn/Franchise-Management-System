using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.CancelWarehouseTransfer;

public sealed record CancelWarehouseTransferCommand(Guid Id, string? Reason, Guid CancelledBy) : IRequest<CancelWarehouseTransferResult>;

public sealed record CancelWarehouseTransferResult(
    Guid Id,
    string TransferCode,
    TransferStatus Status,
    string? Reason,
    DateTimeOffset UpdatedAt
);