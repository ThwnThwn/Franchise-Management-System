using AutoMapper;
using Grpc.Core;
using MediatR;
using SalesService.API;
using SalesService.Application.Features.Orders.Create;
using SalesService.Application.Features.Orders.GetById;
using System;

namespace SalesService.API.Services.Orders;

public class OrderGrpcService : OrderGrpc.OrderGrpcBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public OrderGrpcService(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    public override async Task<CreateOrderReply> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        // Lấy UserId từ Metadata header
        var userIdEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));

        if (userIdEntry == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-user-id in request headers"));
        }

        var customerId = Guid.Parse(userIdEntry.Value);

        // Map từ gRPC request sang DTO
        var createOrderRequest = new CreateOrderRequestDto
        {
            Tax = request.Tax > 0 ? (decimal?)request.Tax : null,
            PaymentMethod = request.PaymentMethod > 0 ? request.PaymentMethod : null,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes,
            CustomerId = customerId,
            Items = request.Items.Select(item => new CartItemDto
            {
                Sku = item.Sku,
                Quantity = item.Quantity
            }).ToList()
        };

        var command = new CreateOrderCommand(createOrderRequest);
        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? "Failed to create order";
            throw new RpcException(new Status(StatusCode.Internal, errorMessage));
        }

        return new CreateOrderReply
        {
            OrderId = result.Value.OrderId.ToString(),
            OrderCode = result.Value.OrderCode,
            TotalAmount = (double)result.Value.TotalAmount,
            CustomerId = customerId.ToString()
        };
    }


public override async Task<GetOrderByIdReply> GetOrderById(
    GetOrderByIdRequest request,
    ServerCallContext context)
{
    if (!Guid.TryParse(request.Id, out var orderId))
    {
        throw new RpcException(
            new Status(StatusCode.InvalidArgument, "Invalid order id"));
    }

    var query = new GetOrderByIdQuery(orderId);
    var result = await _mediator.Send(query, context.CancellationToken);

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
        throw new RpcException(
            new Status(StatusCode.NotFound, result.Errors.FirstOrDefault()));
    }

    if (!result.IsSuccess)
    {
        throw new RpcException(
            new Status(StatusCode.Internal, result.Errors.FirstOrDefault()));
    }

    var dto = result.Value;

    var orderMessage = new Order
    {
        Id = dto.OrderId.ToString(),
        Code = dto.OrderCode,
        Subtotal = (double)dto.Subtotal,
        Tax = (double)dto.Tax,
        TotalAmount = (double)dto.TotalAmount,
        PaidAmount = dto.PaidAmount.HasValue
            ? (double)dto.PaidAmount.Value
            : 0,
        PaymentMethod = dto.PaymentMethod.HasValue ? (int)dto.PaymentMethod.Value : 0,
        Status = dto.Status,
        PaidAt = dto.PaidAt.ToString("O"),
        Notes = dto.Notes ?? string.Empty,
        CreatedAt = dto.CreatedAt.ToString("O"),
        UpdatedAt = dto.UpdatedAt.ToString("O")
    };

    orderMessage.Items.AddRange(dto.Items.Select(i => new OrderItem
    {
        Id = i.Id.ToString(),
        ProductId = i.ProductId.ToString(),
        VariantId = i.VariantId.ToString(),
        Quantity = i.Quantity,
        UnitPrice = (double)i.UnitPrice,
        ProductName = i.ProductName,
        Sku = i.Sku,
        ImageUrl = i.ImageUrl ?? string.Empty
    }));

    return new GetOrderByIdReply
    {
        Order = orderMessage
    };
}


}