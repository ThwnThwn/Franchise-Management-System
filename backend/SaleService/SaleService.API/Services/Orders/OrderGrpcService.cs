using System;
using AutoMapper;
using Grpc.Core;
using MediatR;
using SalesService.API;
using SalesService.Application.Features.Orders.Create;

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
}