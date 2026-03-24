using MediatR;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.CancelWarehouseTransfer;

public class CancelWarehouseTransferCommandHandler : IRequestHandler<CancelWarehouseTransferCommand, CancelWarehouseTransferResult>
{
    private readonly IWarehouseTransferRepository _warehouseTransferRepository;

    public CancelWarehouseTransferCommandHandler(IWarehouseTransferRepository warehouseTransferRepository)
    {
        _warehouseTransferRepository = warehouseTransferRepository;
    }

    public async Task<CancelWarehouseTransferResult> Handle(CancelWarehouseTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _warehouseTransferRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transfer is null)
            throw new KeyNotFoundException($"Warehouse transfer with ID '{request.Id}' was not found.");

        if (transfer.Status != TransferStatus.Pending && transfer.Status != TransferStatus.Approved)
            throw new InvalidOperationException($"Transfer cannot be cancelled. Current status: {transfer.Status}.");

        transfer.Status = TransferStatus.Cancelled;
        transfer.Reason = request.Reason ?? transfer.Reason;
        transfer.UpdatedAt = DateTimeOffset.UtcNow;

        await _warehouseTransferRepository.SaveChangesAsync(cancellationToken);

        return new CancelWarehouseTransferResult(
            transfer.Id,
            transfer.TransferCode,
            transfer.Status,
            transfer.Reason,
            transfer.UpdatedAt
        );
    }
}