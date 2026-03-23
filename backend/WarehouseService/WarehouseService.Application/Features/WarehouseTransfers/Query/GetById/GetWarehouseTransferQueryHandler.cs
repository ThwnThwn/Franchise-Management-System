using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Features.WarehouseTransfers.Query.GetById;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.WarehouseTransfers.Query.GetWarehouseTransfer;

public class GetWarehouseTransferQueryHandler : IRequestHandler<GetWarehouseTransferQuery, GetWarehouseTransferResult>
{
    private readonly IWarehouseTransferRepository _warehouseTransferRepository;
    private readonly IWarehouseTransferItemRepository _warehouseTransferItemRepository;

    public GetWarehouseTransferQueryHandler(
        IWarehouseTransferRepository warehouseTransferRepository,
        IWarehouseTransferItemRepository warehouseTransferItemRepository)
    {
        _warehouseTransferRepository = warehouseTransferRepository;
        _warehouseTransferItemRepository = warehouseTransferItemRepository;
    }

    public async Task<GetWarehouseTransferResult> Handle(GetWarehouseTransferQuery request, CancellationToken cancellationToken)
    {
        var transfer = await _warehouseTransferRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transfer is null)
            throw new KeyNotFoundException($"Warehouse transfer with ID '{request.Id}' was not found.");

        var transferItems = await _warehouseTransferItemRepository
            .GetQueryable()
            .Where(ti => ti.WarehouseTransferId == request.Id)
            .Join(
                _warehouseTransferItemRepository.GetWarehouseItemQueryable(),
                ti => ti.WarehouseItemId,
                wi => wi.Id,
                (ti, wi) => new TransferItemInfoDto(
                    ti.WarehouseItemId,
                    ti.IngredientId,
                    wi.Unit,
                    ti.Quantity,
                    ti.UnitPrice
                )
            )
            .ToListAsync(cancellationToken);

        return new GetWarehouseTransferResult(
            transfer.Id,
            transfer.TransferCode,
            transfer.FromWarehouseId,
            transfer.ToWarehouseId,
            transfer.Status,
            transfer.Reason,
            transfer.Note,
            transfer.CreatedBy,
            transfer.ApprovedBy,
            transfer.ApprovedAt,
            transfer.CreatedAt,
            transfer.UpdatedAt,
            transferItems
        );
    }
}