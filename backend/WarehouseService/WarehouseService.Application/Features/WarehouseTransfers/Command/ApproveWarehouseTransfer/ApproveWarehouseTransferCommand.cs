using MediatR;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.ApproveWarehouseTransfer;

public sealed record ApproveWarehouseTransferCommand(Guid Id, Guid ApprovedBy) : IRequest<ApproveWarehouseTransferResult>;

public sealed record ApproveWarehouseTransferResult(
    Guid Id,
    string TransferCode,
    TransferStatus Status,
    Guid ApprovedBy,
    DateTimeOffset ApprovedAt,
    DateTimeOffset UpdatedAt
);