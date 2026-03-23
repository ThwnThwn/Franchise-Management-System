using Grpc.Core;
using MediatR;
using WarehouseService.Application.Features.WarehouseItems.Command.CreateWarehouseItem;
using WarehouseService.Application.Features.WarehouseItems.Command.UpdateWarehouseItem;
using WarehouseService.Application.Features.WarehouseItems.Command.DeleteWarehouseItem;
using WarehouseService.Application.Features.WarehouseItems.Query.GetById;
using WarehouseService.Application.Features.WarehouseItems.Query.GetWarehouseItems;
using WarehouseService.Grpc.Protos.WarehouseItem;

namespace WarehouseService.API.Services;

public class WarehouseItemGrpcService : WarehouseItemGrpc.WarehouseItemGrpcBase
{
    private readonly IMediator _mediator;

    public WarehouseItemGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<CreateWarehouseItemReply> CreateWarehouseItem(
        CreateWarehouseItemRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.WarehouseId, out var warehouseId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse ID."));
        if (!Guid.TryParse(request.IngredientId, out var productId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Ingredient ID."));

        var result = await _mediator.Send(new CreateWarehouseItemCommand(
            warehouseId, productId, request.Unit,
            (decimal)request.Quantity, (decimal)request.BasePrice,
            string.IsNullOrWhiteSpace(request.ExpiryDate) ? null
                : DateTime.SpecifyKind(DateTime.Parse(request.ExpiryDate), DateTimeKind.Utc)
        ), context.CancellationToken);

        return new CreateWarehouseItemReply
        {
            Id = result.Id.ToString(),
            WarehouseId = result.WarehouseId.ToString(),
            IngredientId = result.IngredientId.ToString(),
            Unit = result.Unit,
            Quantity = (double)result.Quantity,
            BasePrice = (double)result.BasePrice,
            ExpiryDate = result.ExpiryDate?.ToString("o") ?? string.Empty,
            CreatedAt = result.CreatedAt.ToString("o"),
            Message = "Warehouse item created successfully."
        };
    }

    public override async Task<UpdateWarehouseItemReply> UpdateWarehouseItem(
        UpdateWarehouseItemRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse Item ID."));

        var result = await _mediator.Send(new UpdateWarehouseItemCommand(
            id, request.Unit, (decimal)request.BasePrice,
            string.IsNullOrWhiteSpace(request.ExpiryDate) ? null
                : DateTime.SpecifyKind(DateTime.Parse(request.ExpiryDate), DateTimeKind.Utc)
        ), context.CancellationToken);

        return new UpdateWarehouseItemReply
        {
            Id = result.Id.ToString(),
            WarehouseId = result.WarehouseId.ToString(),
            IngredientId = result.IngredientID.ToString(),
            Unit = result.Unit,
            Quantity = (double)result.Quantity,
            BasePrice = (double)result.BasePrice,
            ExpiryDate = result.ExpiryDate?.ToString("o") ?? string.Empty,
            UpdatedAt = result.UpdatedAt.ToString("o"),
            Message = "Warehouse item updated successfully."
        };
    }

    public override async Task<DeleteWarehouseItemReply> DeleteWarehouseItem(
        DeleteWarehouseItemRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse Item ID."));

        await _mediator.Send(new DeleteWarehouseItemCommand(id), context.CancellationToken);
        return new DeleteWarehouseItemReply { Message = "Warehouse item deleted successfully." };
    }

    public override async Task<WarehouseItemDetail> GetWarehouseItemById(
        GetWarehouseItemByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse Item ID."));

        var result = await _mediator.Send(new GetWarehouseItemQuery(id), context.CancellationToken);

        return new WarehouseItemDetail
        {
            Id = result.Id.ToString(),
            WarehouseId = result.WarehouseId.ToString(),
            IngredientId = result.IngredientId.ToString(),
            Unit = result.Unit,
            Quantity = (double)result.Quantity,
            BasePrice = (double)result.BasePrice,
            ExpiryDate = result.ExpiryDate?.ToString("o") ?? string.Empty,
            CreatedAt = result.CreatedAt.ToString("o"),
            UpdatedAt = result.UpdatedAt.ToString("o")
        };
    }

    public override async Task<GetWarehouseItemsReply> GetWarehouseItems(
        GetWarehouseItemsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.WarehouseId, out var warehouseId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse ID."));

        Guid? productId = null;
        if (!string.IsNullOrWhiteSpace(request.IngredientId))
        {
            if (!Guid.TryParse(request.IngredientId, out var parsedProductId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Ingredient ID."));
            productId = parsedProductId;
        }

        var result = await _mediator.Send(new GetWarehouseItemsQuery(
            warehouseId, productId,
            request.HasLowStock ? request.LowStock : null,
            request.HasExpiringSoon ? request.ExpiringSoon : null,
            request.Page, request.PageSize
        ), context.CancellationToken);

        var reply = new GetWarehouseItemsReply
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };

        reply.Items.AddRange(result.Items.Select(i => new WarehouseItemSummary
        {
            Id = i.Id.ToString(),
            WarehouseId = i.WarehouseId.ToString(),
            IngredientId = i.IngredientId.ToString(),
            Unit = i.Unit,
            Quantity = (double)i.Quantity,
            BasePrice = (double)i.BasePrice,
            ExpiryDate = i.ExpiryDate?.ToString("o") ?? string.Empty
        }));

        return reply;
    }
}