using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using MediatR;
using SaleService.API;
using SaleService.Application.Features.Orders.Create;

namespace SaleService.API.Services.Orders;

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
        // Map từ gRPC request sang DTO
        var createOrderRequest = new CreateOrderRequestDto
        {
            Tax = request.Tax > 0 ? (decimal?)request.Tax : null,
            PaymentMethod = request.PaymentMethod > 0 ? (int?)request.PaymentMethod : null,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes,
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
            TotalAmount = (double)result.Value.TotalAmount
        };
    }
}
