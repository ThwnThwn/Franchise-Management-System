using MediatR;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.CancelWarehouseTransfer;

public class CancelWarehouseTransferCommandHandler(
    IWarehouseTransferRepository warehouseTransferRepository
) : IRequestHandler<CancelWarehouseTransferCommand, CancelWarehouseTransferResult>
{
    public async Task<CancelWarehouseTransferResult> Handle(CancelWarehouseTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await warehouseTransferRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transfer is null)
            throw new KeyNotFoundException($"Warehouse transfer with ID '{request.Id}' was not found.");

        if (transfer.Status != TransferStatus.Pending && transfer.Status != TransferStatus.Approved)
            throw new InvalidOperationException($"Transfer cannot be cancelled. Current status: {transfer.Status}.");

        transfer.Status = TransferStatus.Cancelled;
        transfer.Reason = request.Reason ?? transfer.Reason;
        transfer.UpdatedAt = DateTimeOffset.UtcNow;

        await warehouseTransferRepository.SaveChangesAsync(cancellationToken);

        return new CancelWarehouseTransferResult(
            transfer.Id,
            transfer.TransferCode,
            transfer.Status,
            transfer.Reason,
            transfer.UpdatedAt
        );
    }
}