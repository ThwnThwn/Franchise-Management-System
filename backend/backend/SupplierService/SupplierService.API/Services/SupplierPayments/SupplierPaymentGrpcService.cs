using Grpc.Core;
using MediatR;
using SupplierService.Application.Features.SupplierPayments.Commands.CreatePoPayment;
using SupplierService.Application.Features.SupplierPayments.Commands.VnPayIpn;
using SupplierService.Application.Features.SupplierPayments.Commands.VnPayReturn;
using SupplierService.Application.Features.SupplierPayments.Queries.GetAllSupplierPayments;
using SupplierService.Application.Features.SupplierPayments.Queries.GetPaymentsByPurchaseOrder;
using SupplierService.Domain.Enums;
using SupplierService.Grpc.Protos.Supplier;

namespace SupplierService.API.Services.SupplierPayments;

public class SupplierPaymentGrpcService : SupplierPaymentGrpc.SupplierPaymentGrpcBase
{
    private readonly IMediator _mediator;

    public SupplierPaymentGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Ghi nhận thanh toán cho đơn nhập hàng.
    /// Cash → thanh toán tức thì, url = "".
    /// VNPay → tạo Pending payment, trả về URL redirect.
    /// </summary>
    public override async Task<CreatePoPaymentReply> CreatePoPayment(
        CreatePoPaymentRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.PurchaseOrderId, out var poId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid purchase_order_id"));

        var method = request.Method switch
        {
            SupplierPaymentMethodType.Cash => SupplierPaymentMethod.Cash,
            SupplierPaymentMethodType.Vnpay => SupplierPaymentMethod.VNPay,
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid payment method"))
        };

        var command = new CreatePoPaymentCommand(
            PurchaseOrderId: poId,
            Method: method,
            ReturnUrl: request.HasReturnUrl ? request.ReturnUrl : null
        );


        var result = await _mediator.Send(command, context.CancellationToken);

        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            throw new RpcException(new Status(StatusCode.NotFound, result.Errors.FirstOrDefault() ?? "Purchase order not found"));

        if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            throw new RpcException(new Status(StatusCode.InvalidArgument, result.ValidationErrors.FirstOrDefault()?.ErrorMessage ?? "Validation error"));

        if (!result.IsSuccess)
            throw new RpcException(new Status(StatusCode.Internal, result.Errors.FirstOrDefault() ?? "Failed to record payment"));

        return new CreatePoPaymentReply
        {
            PaymentId = result.Value.PaymentId.ToString(),
            Code = result.Value.Code,
            Url = result.Value.Url
        };
    }

    /// <summary>
    /// VNPay callback: xác thực chữ ký và cập nhật trạng thái payment + PO.
    /// </summary>
    public override async Task<HandleVnPayReturnReply> HandleVnPayReturn(
        HandleVnPayReturnRequest request,
        ServerCallContext context)
    {
        var queryParams = request.QueryParams.ToDictionary(kv => kv.Key, kv => kv.Value);
        var command = new VnPayReturnCommand(queryParams);

        var result = await _mediator.Send(command, context.CancellationToken);

        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            throw new RpcException(new Status(StatusCode.NotFound, result.Errors.FirstOrDefault() ?? "Payment not found"));

        if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            throw new RpcException(new Status(StatusCode.InvalidArgument, result.ValidationErrors.FirstOrDefault()?.ErrorMessage ?? "Validation error"));

        if (!result.IsSuccess)
            throw new RpcException(new Status(StatusCode.Internal, result.Errors.FirstOrDefault() ?? "VNPay verification failed"));

        return new HandleVnPayReturnReply
        {
            Success = result.Value.Success,
            PaymentId = result.Value.PaymentId.ToString(),
            PurchaseOrderId = result.Value.PurchaseOrderId.ToString(),
            ResponseCode = result.Value.ResponseCode
        };
    }

    /// <summary>
    /// Lấy tất cả payments của 1 đơn nhập hàng.
    /// </summary>
    public override async Task<GetPaymentsByPurchaseOrderReply> GetPaymentsByPurchaseOrder(
        GetPaymentsByPurchaseOrderRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.PurchaseOrderId, out var poId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid purchase_order_id"));

        var query = new GetPaymentsByPurchaseOrderQuery(poId);
        var result = await _mediator.Send(query, context.CancellationToken);

        var reply = new GetPaymentsByPurchaseOrderReply();
        reply.Payments.AddRange(result.Payments.Select(p => new SupplierService.Grpc.Protos.Supplier.SupplierPaymentDto
        {
            Id = p.Id,
            PurchaseOrderId = p.PurchaseOrderId,
            Code = p.Code,
            Amount = p.Amount,
            Method = p.Method,
            Status = p.Status,
            Notes = p.Notes ?? string.Empty,
            PaidAt = p.PaidAt ?? string.Empty,
            CreatedAt = p.CreatedAt
        }));

        return reply;
    }

    /// <summary>
    /// Lấy tất cả supplier payments có phân trang, lọc theo supplier/PO.
    /// </summary>
    public override async Task<GetAllSupplierPaymentsReply> GetAllSupplierPayments(
        GetAllSupplierPaymentsRequest request,
        ServerCallContext context)
    {
        var query = new GetAllSupplierPaymentsQuery(
            Page: request.Page <= 0 ? 1 : request.Page,
            PageSize: request.PageSize <= 0 ? 10 : request.PageSize,
            SupplierId: request.HasSupplierId ? request.SupplierId : null,
            PurchaseOrderId: request.HasPurchaseOrderId ? request.PurchaseOrderId : null
        );

        var result = await _mediator.Send(query, context.CancellationToken);

        var reply = new GetAllSupplierPaymentsReply { TotalCount = result.TotalCount };
        reply.Payments.AddRange(result.Payments.Select(p => new SupplierService.Grpc.Protos.Supplier.SupplierPaymentSummaryDto
        {
            Id = p.Id,
            PurchaseOrderId = p.PurchaseOrderId,
            PurchaseOrderCode = p.PurchaseOrderCode,
            Code = p.Code,
            Amount = p.Amount,
            Method = p.Method,
            Status = p.Status,
            PaidAt = p.PaidAt ?? string.Empty,
            CreatedAt = p.CreatedAt
        }));

        return reply;
    }

    /// <summary>
    /// VNPay IPN: xác thực chữ ký và cập nhật trạng thái payment + PO.
    /// Gateway gọi xuống sau khi nhận IPN callback từ VNPay.
    /// </summary>
    public override async Task<HandleVnPayIpnReply> HandleVnPayIpn(
        HandleVnPayIpnRequest request,
        ServerCallContext context)
    {
        var queryParams = request.QueryParams.ToDictionary(kv => kv.Key, kv => kv.Value);
        var command = new VnPayIpnCommand(queryParams);

        var result = await _mediator.Send(command, context.CancellationToken);

        if (result.IsSuccess && result.Value != null)
        {
            return new HandleVnPayIpnReply
            {
                RspCode = result.Value.RspCode,
                Message = result.Value.Message
            };
        }

        return new HandleVnPayIpnReply
        {
            RspCode = "99",
            Message = result.Errors.FirstOrDefault() ?? "Unknown error"
        };
    }
}
