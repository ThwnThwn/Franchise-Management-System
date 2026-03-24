using MediatR;
using WarehouseService.Application.ClientInterfaces;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;

namespace WarehouseService.Application.Features.Warehouses.Command.CreateWarehouse;

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, CreateWarehouseResult>
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IFranchiseServiceClient _franchiseServiceClient;
    public CreateWarehouseCommandHandler(IWarehouseRepository warehouseRepository, IFranchiseServiceClient franchiseServiceClient)
    {
        _warehouseRepository = warehouseRepository;
        _franchiseServiceClient = franchiseServiceClient;   
    }

    public async Task<CreateWarehouseResult> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var store = await _franchiseServiceClient.StoreExistsAsync(request.StoreId);
        if (store is false)
            throw new KeyNotFoundException($"Store with Id '{request.StoreId}' not found.");

        var warehouseExistsWithStoreId=await _warehouseRepository.ExistWithStoreId(request.StoreId, cancellationToken: cancellationToken);
        if(warehouseExistsWithStoreId is true)
            throw new InvalidOperationException($"Warehouse with Store Id '{request.StoreId}' already existed.");

        var codeExists = await _warehouseRepository.CodeExistsAsync(request.Code, cancellationToken: cancellationToken);
        if (codeExists)
            throw new InvalidOperationException($"Warehouse Code '{request.Code}' already existed.");

        var addressExists = await _warehouseRepository.ExistWithStoreAddress(request.Address, cancellationToken: cancellationToken);
        if(addressExists is true)
            throw new InvalidOperationException($"Warehouse with Address '{request.Address}' already existed.");

        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Code = request.Code.ToUpper().Trim(),
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            StoreId = request.StoreId,
            Status = request.Status,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _warehouseRepository.AddAsync(warehouse, cancellationToken);
        await _warehouseRepository.SaveChangesAsync(cancellationToken);

        return new CreateWarehouseResult(
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            warehouse.Address,
            warehouse.StoreId,
            warehouse.Status,
            warehouse.CreatedAt
        );
    }
}