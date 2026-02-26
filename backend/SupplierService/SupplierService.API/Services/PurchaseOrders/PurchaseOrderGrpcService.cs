using Grpc.Core;
using MediatR;
using SupplierService.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;
using SupplierService.Application.Features.PurchaseOrders.Commands.CompletePurchaseOrder;
using SupplierService.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using SupplierService.Application.Features.PurchaseOrders.Commands.UpdatePurchaseOrder;
using SupplierService.Grpc.Protos.Supplier;
using SupplierService.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using SupplierService.Domain.Enums;
using SupplierService.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using SupplierService.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;
namespace SupplierService.API.Services.PurchaseOrders;

public class PurchaseOrderGrpcService : PurchaseOrderGrpc.PurchaseOrderGrpcBase
{
    private readonly IMediator _mediator;
    private readonly ISupplierDbContext _dbContext;

    public PurchaseOrderGrpcService(IMediator mediator, ISupplierDbContext dbContext)
    {
        _mediator = mediator;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Hoàn thành đơn nhập hàng → cập nhật PO = Completed, cộng kho vào InventoryService.
    /// </summary>
    public override async Task<CompletePurchaseOrderReply> CompletePurchaseOrder(
        CompletePurchaseOrderRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.PurchaseOrderId, out var poId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid PurchaseOrderId"));
        }

        var command = new CompletePurchaseOrderCommand(poId);
        await _mediator.Send(command, context.CancellationToken);

        return new CompletePurchaseOrderReply
        {
            Message = "Purchase order completed and stock increased successfully."
        };
    }

    /// <summary>
    /// Lấy danh sách đơn nhập hàng (có phân trang, lọc theo supplier/store/status).
    /// </summary>
    public override async Task<GetPurchaseOrdersReply> GetPurchaseOrders(
        GetPurchaseOrdersRequest request, ServerCallContext context)
    {
        var query = new GetPurchaseOrdersQuery(
            request.HasSupplierId ? request.SupplierId : null,
            request.HasStoreId ? request.StoreId : null,
            request.HasStatus ? request.Status : null,
            request.Page,
            request.PageSize
        );

        var result = await _mediator.Send(query, context.CancellationToken);

        var reply = new GetPurchaseOrdersReply
        {
            TotalCount = result.TotalCount
        };

        foreach (var po in result.PurchaseOrders)
        {
            reply.PurchaseOrders.Add(new PurchaseOrderSummary
            {
                Id = po.Id,
                Code = po.Code,
                SupplierName = po.SupplierName,
                Status = po.Status,
                PaymentStatus = po.PaymentStatus,
                TotalAmount = po.TotalAmount,
                NetAmount = po.NetAmount,
                OrderDate = po.OrderDate,
                CreatedAt = po.CreatedAt
            });
        }

        return reply;
    }

    /// <summary>
    /// Lấy chi tiết 1 đơn nhập hàng theo ID.
    /// </summary>
    public override async Task<PurchaseOrderDetail> GetPurchaseOrderById(
        GetPurchaseOrderByIdRequest request, ServerCallContext context)
    {
        var query = new GetPurchaseOrderByIdQuery(request.Id);
        var detailDto = await _mediator.Send(query, context.CancellationToken);

        var detail = new PurchaseOrderDetail
        {
            Id = detailDto.Id,
            Code = detailDto.Code,
            SupplierId = detailDto.SupplierId,
            SupplierName = detailDto.SupplierName,
            StoreId = detailDto.StoreId,
            Status = detailDto.Status,
            PaymentStatus = detailDto.PaymentStatus,
            TotalAmount = detailDto.TotalAmount,
            DiscountAmount = detailDto.DiscountAmount,
            NetAmount = detailDto.NetAmount,
            PaidAmount = detailDto.PaidAmount,
            OrderDate = detailDto.OrderDate,
            CompletedAt = detailDto.CompletedAt,
            Notes = detailDto.Notes,
            CreatedAt = detailDto.CreatedAt,
            UpdatedAt = detailDto.UpdatedAt
        };

        foreach (var item in detailDto.Items)
        {
            detail.Items.Add(new PurchaseOrderItemDetail
            {
                Id = item.Id,
                IngredientId = item.IngredientId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            });
        }

        return detail;
    }

    /// <summary>
    /// Tạo mới đơn nhập hàng.
    /// </summary>
    public override async Task<CreatePurchaseOrderReply> CreatePurchaseOrder(
        CreatePurchaseOrderRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.SupplierId, out var supplierId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid SupplierId"));
        }

        Guid? storeId = null;
        if (!string.IsNullOrEmpty(request.StoreId) && Guid.TryParse(request.StoreId, out var sid))
        {
            storeId = sid;
        }

        DateTimeOffset? expectedDeliveryDate = null;
        if (!string.IsNullOrEmpty(request.ExpectedDeliveryDate))
        {
            if (DateTimeOffset.TryParse(request.ExpectedDeliveryDate, out var edd))
                expectedDeliveryDate = edd;
        }

        var items = request.Items.Select(i =>
        {
            if (!Guid.TryParse(i.IngredientId, out var ingredientId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid IngredientId: {i.IngredientId}"));

            return new CreatePurchaseOrderItemDto(ingredientId, (decimal)i.Quantity);
        }).ToList();

        var command = new CreatePurchaseOrderCommand(
            SupplierId: supplierId,
            StoreId: storeId,
            Notes: request.HasNotes ? request.Notes : null,
            ReferenceCode: request.HasReferenceCode ? request.ReferenceCode : null,
            ExpectedDeliveryDate: expectedDeliveryDate,
            DiscountAmount: (decimal)request.DiscountAmount,
            Items: items
        );

        var result = await _mediator.Send(command, context.CancellationToken);

        return new CreatePurchaseOrderReply
        {
            Id = result.Id.ToString(),
            Code = result.Code
        };
    }

    /// <summary>
    /// Cập nhật đơn nhập hàng (chỉ khi Status = Pending).
    /// </summary>
    public override async Task<UpdatePurchaseOrderReply> UpdatePurchaseOrder(
        UpdatePurchaseOrderRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var poId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid PurchaseOrder Id"));
        }

        if (!Guid.TryParse(request.SupplierId, out var supplierId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid SupplierId"));
        }

        Guid? storeId = null;
        if (!string.IsNullOrEmpty(request.StoreId) && Guid.TryParse(request.StoreId, out var sid))
        {
            storeId = sid;
        }

        DateTimeOffset? expectedDeliveryDate = null;
        if (!string.IsNullOrEmpty(request.ExpectedDeliveryDate))
        {
            if (DateTimeOffset.TryParse(request.ExpectedDeliveryDate, out var edd))
                expectedDeliveryDate = edd;
        }

        var items = request.Items.Select(i =>
        {
            if (!Guid.TryParse(i.IngredientId, out var ingredientId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid IngredientId: {i.IngredientId}"));

            return new CreatePurchaseOrderItemDto(ingredientId, (decimal)i.Quantity);
        }).ToList();

        var command = new UpdatePurchaseOrderCommand(
            Id: poId,
            SupplierId: supplierId,
            StoreId: storeId,
            Notes: request.HasNotes ? request.Notes : null,
            ReferenceCode: request.HasReferenceCode ? request.ReferenceCode : null,
            ExpectedDeliveryDate: expectedDeliveryDate,
            DiscountAmount: (decimal)request.DiscountAmount,
            Items: items
        );

        await _mediator.Send(command, context.CancellationToken);

        return new UpdatePurchaseOrderReply
        {
            Message = "Purchase order updated successfully."
        };
    }

    /// <summary>
    /// Hủy đơn nhập hàng (chỉ khi Pending hoặc Processing).
    /// </summary>
    public override async Task<CancelPurchaseOrderReply> CancelPurchaseOrder(
        CancelPurchaseOrderRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var poId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid PurchaseOrder Id"));
        }

        var command = new CancelPurchaseOrderCommand(
            Id: poId,
            Reason: request.HasReason ? request.Reason : null
        );

        await _mediator.Send(command, context.CancellationToken);

        return new CancelPurchaseOrderReply
        {
            Message = "Purchase order cancelled successfully."
        };
    }
}

