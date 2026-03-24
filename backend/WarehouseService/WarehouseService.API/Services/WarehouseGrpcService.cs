using Grpc.Core;
using MediatR;
using WarehouseService.Application.Features.Warehouses.Command.CreateWarehouse;
using WarehouseService.Application.Features.Warehouses.Command.DeleteWarehouse;
using WarehouseService.Application.Features.Warehouses.Command.UpdateWarehouse;
using WarehouseService.Application.Features.Warehouses.Queries.GetAll;
using WarehouseService.Application.Features.Warehouses.Queries.GetById;
using WarehouseService.Grpc.Protos.Warehouse;
using DomainWarehouseStatus = WarehouseService.Domain.Enums.WarehouseStatus;

namespace WarehouseService.API.Services;

public class WarehouseGrpcService : WarehouseGrpc.WarehouseGrpcBase
{
    private readonly IMediator _mediator;

    public WarehouseGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<CreateWarehouseReply> CreateWarehouse(
        CreateWarehouseRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.StoreId, out var storeId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Store ID."));

        var result = await _mediator.Send(new CreateWarehouseCommand(
            request.Code, request.Name, request.Address,
            storeId, ToDomainStatus((WarehouseStatus)request.Status)
        ), context.CancellationToken);

        return new CreateWarehouseReply
        {
            Id = result.Id.ToString(),
            Code = result.Code,
            Name = result.Name,
            Address = result.Address,
            StoreId = result.StoreId.ToString(),
            Status = (int)ToProtoStatus(result.Status),
            CreatedAt = result.CreatedAt.ToString("o"),
            Message = "Warehouse created successfully."
        };
    }

    public override async Task<UpdateWarehouseReply> UpdateWarehouse(
        UpdateWarehouseRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse ID."));

        Guid? storeId = null;
        if (request.HasStoreId && !string.IsNullOrWhiteSpace(request.StoreId))
        {
            if (!Guid.TryParse(request.StoreId, out var parsedStoreId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Store ID."));
            storeId = parsedStoreId;
        }

        DomainWarehouseStatus? status = null;
        if (request.HasStatus)
            status = ToDomainStatus(request.Status);

        var result = await _mediator.Send(new UpdateWarehouseCommand(
            id,
            request.HasCode && !string.IsNullOrWhiteSpace(request.Code) ? request.Code : null,
            request.HasName && !string.IsNullOrWhiteSpace(request.Name) ? request.Name : null,
            request.HasAddress && !string.IsNullOrWhiteSpace(request.Address) ? request.Address : null,
            status,
            storeId
        ), context.CancellationToken);

        return new UpdateWarehouseReply
        {
            Id = result.Id.ToString(),
            Code = result.Code,
            Name = result.Name,
            Address = result.Address,
            StoreId = result.StoreId.ToString(),
            Status = (int)ToProtoStatus(result.Status),
            UpdatedAt = result.UpdatedAt.ToString("o"),
            Message = "Warehouse updated successfully."
        };
    }

    public override async Task<DeleteWarehouseReply> DeleteWarehouse(
        DeleteWarehouseRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse ID."));

        await _mediator.Send(new DeleteWarehouseCommand(id), context.CancellationToken);
        return new DeleteWarehouseReply { Message = "Warehouse deleted successfully." };
    }

    public override async Task<WarehouseDetail> GetWarehouseById(
        GetWarehouseByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse ID."));

        var result = await _mediator.Send(new GetWarehouseQuery(id), context.CancellationToken);

        return new WarehouseDetail
        {
            Id = result.Id.ToString(),
            Code = result.Code,
            Name = result.Name,
            Address = result.Address,
            StoreId = result.StoreId.ToString(),
            Status = (int)ToProtoStatus(result.Status),
            CreatedAt = result.CreatedAt.ToString("o"),
            UpdatedAt = result.UpdatedAt.ToString("o")
        };
    }

    public override async Task<GetWarehousesReply> GetWarehouses(
        GetWarehousesRequest request, ServerCallContext context)
    {
        Guid? storeId = null;
        if (request.HasStoreId)
        {
            if (!Guid.TryParse(request.StoreId, out var parsedStoreId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Store ID."));
            storeId = parsedStoreId;
        }

        DomainWarehouseStatus? status = null;
        if (request.HasStatus)
            status = ToDomainStatus(request.Status);

        var result = await _mediator.Send(new GetWarehousesQuery(
            storeId,
            status,
            request.HasSearchTerm ? request.SearchTerm : null,
            request.Page > 0 ? request.Page : 1,
            request.PageSize > 0 ? request.PageSize : 10
        ), context.CancellationToken);

        var reply = new GetWarehousesReply
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };

        reply.Warehouses.AddRange(result.Items.Select(w => new WarehouseSummary
        {
            Id = w.Id.ToString(),
            Code = w.Code,
            Name = w.Name,
            StoreId = w.StoreId.ToString(),
            Status = (int)ToProtoStatus(w.Status)
        }));

        return reply;
    }


    private static WarehouseStatus ToProtoStatus(DomainWarehouseStatus domain) =>
        domain switch
        {
            DomainWarehouseStatus.Active => WarehouseStatus.Active,
            DomainWarehouseStatus.Inactive => WarehouseStatus.Inactive,
            DomainWarehouseStatus.UnderMaintenance => WarehouseStatus.UnderMaintenance,
            _ => WarehouseStatus.Active
        };

    private static DomainWarehouseStatus ToDomainStatus(WarehouseStatus proto) =>
        proto switch
        {
            WarehouseStatus.Active => DomainWarehouseStatus.Active,
            WarehouseStatus.Inactive => DomainWarehouseStatus.Inactive,
            WarehouseStatus.UnderMaintenance => DomainWarehouseStatus.UnderMaintenance,
            _ => DomainWarehouseStatus.Active
        };
}