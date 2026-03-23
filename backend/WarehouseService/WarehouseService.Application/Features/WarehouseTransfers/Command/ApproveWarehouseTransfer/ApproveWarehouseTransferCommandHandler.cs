using MediatR;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.ApproveWarehouseTransfer;

public class ApproveWarehouseTransferCommandHandler : IRequestHandler<ApproveWarehouseTransferCommand, ApproveWarehouseTransferResult>
{
    private readonly IWarehouseTransferRepository _warehouseTransferRepository;

    public ApproveWarehouseTransferCommandHandler(IWarehouseTransferRepository warehouseTransferRepository)
    {
        _warehouseTransferRepository = warehouseTransferRepository;
    }

    public async Task<ApproveWarehouseTransferResult> Handle(ApproveWarehouseTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _warehouseTransferRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transfer is null)
            throw new KeyNotFoundException($"Warehouse transfer with ID '{request.Id}' was not found.");

        if (transfer.Status != TransferStatus.Pending)
            throw new InvalidOperationException($"Only pending transfers can be approved. Current status: {transfer.Status}.");

        // Approve
        var approvedAt = DateTimeOffset.UtcNow;
        transfer.Status = TransferStatus.Approved;
        transfer.ApprovedAt = approvedAt.UtcDateTime;
        transfer.UpdatedAt = approvedAt;
        transfer.ApprovedBy = request.ApprovedBy;

        await _warehouseTransferRepository.SaveChangesAsync(cancellationToken);

        return new ApproveWarehouseTransferResult(
            transfer.Id,
            transfer.TransferCode,
            transfer.Status,
            transfer.ApprovedBy.Value,
            approvedAt,
            transfer.UpdatedAt
        );
    }
} 