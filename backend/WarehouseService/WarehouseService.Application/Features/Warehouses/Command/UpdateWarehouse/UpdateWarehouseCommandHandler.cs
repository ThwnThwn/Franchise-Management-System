using MediatR;
using WarehouseService.Application.ClientInterfaces;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Features.Warehouses.Command.UpdateWarehouse;

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, UpdateWarehouseResult>
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IFranchiseServiceClient _franchiseServiceClient;

    public UpdateWarehouseCommandHandler(
        IWarehouseRepository warehouseRepository,
        IFranchiseServiceClient franchiseServiceClient)
    {
        _warehouseRepository = warehouseRepository;
        _franchiseServiceClient = franchiseServiceClient;
    }

    public async Task<UpdateWarehouseResult> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse is null)
            throw new KeyNotFoundException($"Warehouse with ID '{request.Id}' was not found.");

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var codeExists = await _warehouseRepository.CodeExistsAsync(
                request.Code, excludeId: request.Id, cancellationToken: cancellationToken);
            if (codeExists)
                throw new InvalidOperationException($"Warehouse code '{request.Code}' already exists.");

            warehouse.Code = request.Code.ToUpper().Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            warehouse.Name = request.Name.Trim();

        if (!string.IsNullOrWhiteSpace(request.Address))
        {
            var addressExists = await _warehouseRepository.ExistWithStoreAddress(
                request.Address, cancellationToken: cancellationToken);
            if (addressExists)
                throw new InvalidOperationException($"Warehouse with address '{request.Address}' already exists.");

            warehouse.Address = request.Address.Trim();
        }

        if (request.Status.HasValue)
            warehouse.Status = request.Status.Value;

        if (request.StoreId.HasValue && request.StoreId.Value != warehouse.StoreId)
        {
            var storeExists = await _franchiseServiceClient.StoreExistsAsync(request.StoreId.Value);
            if (!storeExists)
                throw new KeyNotFoundException($"Store with ID '{request.StoreId.Value}' was not found.");

            var warehouseExistsWithStoreId = await _warehouseRepository.ExistWithStoreId(
                request.StoreId.Value, cancellationToken: cancellationToken);
            if (warehouseExistsWithStoreId)
                throw new InvalidOperationException($"A warehouse with store ID '{request.StoreId}' already exists.");

            warehouse.StoreId = request.StoreId.Value;
        }

        warehouse.UpdatedAt = DateTimeOffset.UtcNow;

        await _warehouseRepository.SaveChangesAsync(cancellationToken);

        return new UpdateWarehouseResult(
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            warehouse.Address,
            warehouse.StoreId,
            warehouse.Status,
            warehouse.UpdatedAt
        );
    }
}