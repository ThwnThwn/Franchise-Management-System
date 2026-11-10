using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.WarehouseTransfers.Query.GetById;

public class GetWarehouseTransferQueryHandler(
    IWarehouseTransferRepository warehouseTransferRepository,
    IWarehouseTransferItemRepository warehouseTransferItemRepository
) : IRequestHandler<GetWarehouseTransferQuery, GetWarehouseTransferResult>
{
    public async Task<GetWarehouseTransferResult> Handle(GetWarehouseTransferQuery request, CancellationToken cancellationToken)
    {
        var transfer = await warehouseTransferRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transfer is null)
            throw new KeyNotFoundException($"Warehouse transfer with ID '{request.Id}' was not found.");

        var transferItems = await warehouseTransferItemRepository
            .GetQueryable()
            .Where(ti => ti.WarehouseTransferId == request.Id)
            .Join(
                warehouseTransferItemRepository.GetWarehouseItemQueryable(),
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