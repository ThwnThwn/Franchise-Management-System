using AutoMapper;
using Grpc.Core;
using MediatR;
using SalesService.Application.Features.Orders.Commands.Create;
using SalesService.Application.Features.Orders.Queries.GetMyOrders;

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


    public override async Task<GetMyOrdersReply> GetMyOrders(GetMyOrdersRequest request, ServerCallContext context)
    {
        //Lấy x-user-id từ metadata
        var userIdEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));

        if (userIdEntry == null)
        {
            throw new RpcException(new Status(
                StatusCode.Unauthenticated,
                "Missing x-user-id in request headers"));
        }

        var customerId = Guid.Parse(userIdEntry.Value);

        //phân trang
        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;

        var query = new GetMyOrdersQuery(customerId, page, pageSize);

        var result = await _mediator.Send(query, context.CancellationToken);

        // Map sang gRPC response
        var reply = new GetMyOrdersReply
        {
            TotalCount = result.TotalCount
        };

        foreach (var order in result.Orders)
        {
            var orderSummary = new OrderSummary
            {
                Id = order.Id.ToString(),
                Code = order.Code,
                Subtotal = (double)order.Subtotal,
                TotalAmount = (double)order.TotalAmount,
                Status = order.Status,
                PaidAt = order.PaidAt.ToString("O"),
                CreatedAt = order.CreatedAt.ToString("O"),
                UpdatedAt = order.UpdatedAt.ToString("O"),
                CustomerId = order.CustomerId?.ToString() ?? "",

                //dùng -1 cho NULL (chưa thanh toán)
                Tax = order.Tax.HasValue ? (double)order.Tax.Value : -1,
                PaidAmount = order.PaidAmount.HasValue ? (double)order.PaidAmount.Value : -1,
                PaymentMethod = order.PaymentMethod ?? -1
            };

            if (!string.IsNullOrEmpty(order.Notes))
            {
                orderSummary.Notes = order.Notes;
            }

            reply.Orders.Add(orderSummary);
        }

        return reply;
    }
}