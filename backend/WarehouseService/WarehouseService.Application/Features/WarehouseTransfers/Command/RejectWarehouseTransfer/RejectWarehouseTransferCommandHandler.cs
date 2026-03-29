using MediatR;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.RejectWarehouseTransfer;

public class RejectWarehouseTransferCommandHandler(
    IWarehouseTransferRepository warehouseTransferRepository
) : IRequestHandler<RejectWarehouseTransferCommand, RejectWarehouseTransferResult>
{
    public async Task<RejectWarehouseTransferResult> Handle(RejectWarehouseTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await warehouseTransferRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transfer is null)
            throw new KeyNotFoundException($"Warehouse transfer with ID '{request.Id}' was not found.");

        if (transfer.Status != TransferStatus.Pending)
            throw new InvalidOperationException($"Only pending transfers can be rejected. Current status: {transfer.Status}.");

        transfer.Status = TransferStatus.Rejected;
        transfer.Reason = request.Reason;
        transfer.UpdatedAt = DateTimeOffset.UtcNow;

        await warehouseTransferRepository.SaveChangesAsync(cancellationToken);

        return new RejectWarehouseTransferResult(
            transfer.Id,
            transfer.TransferCode,
            transfer.Status,
            transfer.Reason,
            transfer.UpdatedAt
        );
    }
}