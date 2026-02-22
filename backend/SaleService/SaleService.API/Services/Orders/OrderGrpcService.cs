using AutoMapper;
using Grpc.Core;
using InventoryService.Grpc.Protos.Inventory.Internal;
using MediatR;
using SalesService.API;
using SalesService.Application.Features.Orders.Commands.Create;
using SalesService.Application.Features.Orders.Commands.Update;
using SalesService.Application.Features.Orders.Commands.UpdateOrderStatus;
using SalesService.Application.Features.Orders.GetById;
using SalesService.Application.Features.Orders.Queries.GetMyOrders;
using SalesService.Application.Features.Orders.Queries.GetOrders;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
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

    public override async Task<OrderReply> CreateOrder(CreateOrderRequest request, ServerCallContext context)
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
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes,
            CustomerId = customerId,
            StoreId = string.IsNullOrWhiteSpace(request.StoreId) ? null : Guid.Parse(request.StoreId),
            Items = request.Items.Select(item => new CartItemDto
            {
                Sku = item.Sku,
                Quantity = item.Quantity
            }).ToList()
        };

        var command = new CreateOrderCommand(createOrderRequest);
        var result = await _mediator.Send(command, context.CancellationToken);

        //if (!result.IsSuccess)
        //{
        //    var errorMessage = result.Errors.FirstOrDefault() ?? "Failed to create order";
        //    throw new RpcException(new Status(StatusCode.Internal, errorMessage));
        //}

        var reply = new OrderReply
        {
            OrderId = result.Value.OrderId.ToString(),
            OrderCode = result.Value.OrderCode ?? string.Empty, // Tránh lỗi null cũ
            TotalAmount = (double)result.Value.TotalAmount,
            CustomerId = customerId.ToString(),
            CheckStockRespone = new CheckEnoughStockResponse
            {
                IsEnough = result.Value.CheckEnoughStockRes.IsEnough
            }
        };

        // Sử dụng AddRange để đẩy dữ liệu vào list read-only
        if (result.Value.CheckEnoughStockRes.ShortageItems != null)
        {
            // 1. Dùng AddRange vì ShortageItems là read-only (chỉ có get)
            reply.CheckStockRespone.ShortageItems.AddRange(
                result.Value.CheckEnoughStockRes.ShortageItems.Select(i => new ShortageItem
                {
                    // 2. Map đúng tên thuộc tính của ShortageItem trong Sales API
                    // Ví dụ: có thể là Id, VariantId hoặc ProductId tùy vào file proto của Sales
                    VariantId = i.VariantId.ToString(),
                    MaxFeasibleQuantity = i.Amount
                })
            );
        }

        return reply;
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
            TotalAmount = (double)dto.TotalAmount,
            //dùng -1 cho NULL (chưa thanh toán)
            Tax = dto.Tax.HasValue ? (double)dto.Tax :-1,
            PaidAmount = dto.PaidAmount.HasValue? (double)dto.PaidAmount.Value: -1,
            PaymentMethod = dto.PaymentMethod ?? -1,
            Status = dto.Status,
            PaidAt = dto.PaidAt.ToString("O"),
            Notes = dto.Notes ?? string.Empty,
            CreatedAt = dto.CreatedAt.ToString("O"),
            UpdatedAt = dto.UpdatedAt.ToString("O"),
            CustomerId = dto.CustomerId.ToString(),
            
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

    public override async Task<UpdateOrderStatusReply> UpdateOrderStatus(UpdateOrderStatusRequest request,ServerCallContext context)
    {
        //Validate orderId
        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,"Invalid OrderId"));
        }

        //Validate status enum
        if (!Enum.IsDefined(typeof(OrderStatus), request.Status))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid OrderStatus value"
            ));
        }

        //Send MediatR command
        var command = new UpdateOrderStatusCommand(
            new UpdateOrderStatusReq
            {
                OrderId = orderId,
                Status = (Domain.Enums.OrderStatus)(int)request.Status
            }
        );

        var resultMessage = await _mediator.Send(command, context.CancellationToken);

        return new UpdateOrderStatusReply
        {
            Message = resultMessage
        };
    }
    public override async Task<OrderReply> UpdateOrder(UpdateOrderRequest request, ServerCallContext context)
    {
        var userIdEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));
        var roleSlug = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-role-slug", StringComparison.OrdinalIgnoreCase));

        if (userIdEntry == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-user-id in request headers"));
        }

        if (roleSlug == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-role-slug in request headers"));
        }

        var command = new UpdateOrderCommand(new UpdateOrderReq
        {
            OrderId = Guid.Parse(request.OrderId),

            Items = request.Items.Select(x => new Application.Features.Orders.Commands.Update.OrderItem
            {
                Sku = x.Sku,
                Quantity = x.Quantity
            }).ToList(),
            UserId = Guid.Parse(userIdEntry.Value),
            RoleSlug = roleSlug.Value
            
        });
        var result = await _mediator.Send(command);
        var reply = new OrderReply
        {
            OrderId = result.OrderId.ToString(),
            OrderCode = result.OrderCode ?? "",
            TotalAmount = (double)(result.TotalAmount ?? 0),
            CustomerId = result.CustomerId.ToString(),
            CheckStockRespone = new CheckEnoughStockResponse
            {
                IsEnough = result.CheckEnoughStockRes.IsEnough
            }
        };

        // Add shortage items if any
        if (result.CheckEnoughStockRes.ShortageItems != null)
        {
            reply.CheckStockRespone.ShortageItems.AddRange(
                result.CheckEnoughStockRes.ShortageItems.Select(x =>
                    new ShortageItem
                    {
                        VariantId = x.VariantId.ToString(),
                        MaxFeasibleQuantity = x.Amount
                    })
            );
        }

        return reply;
    }

    public override async Task<GetOrdersReply> GetOrders(GetOrdersRequest request, ServerCallContext context)
    {
        var storeId = string.IsNullOrEmpty(request.StoreId) ? (Guid?)null : Guid.Parse(request.StoreId);
        var fromDate = string.IsNullOrEmpty(request.FromDate) ? (DateOnly?)null : DateOnly.Parse(request.FromDate);
        var toDate = string.IsNullOrEmpty(request.ToDate) ? (DateOnly?)null : DateOnly.Parse(request.ToDate);
        var status = request.HasStatus ? (Domain.Enums.OrderStatus?)request.Status : null;
        var paymentMethod = request.HasPaymentMethod ? (Domain.Enums.PaymentMethod?)request.PaymentMethod : null;

        var query = new GetOrdersQuery(
            storeId,
            request.Code,
            fromDate,
            toDate,
            status,
            paymentMethod,
            request.Page > 0 ? request.Page : 1,
            request.PageSize > 0 ? request.PageSize : 10
        );

        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
        {
            throw new RpcException(new Status(StatusCode.Internal, result.Errors.FirstOrDefault() ?? "Failed to get orders"));
        }

        var reply = new GetOrdersReply
        {
            TotalCount = result.Value.TotalCount
        };

        foreach (var order in result.Value.Orders)
        {
            var summary = new OrderSummary
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
                Tax = order.Tax.HasValue ? (double)order.Tax.Value : -1,
                PaidAmount = order.PaidAmount.HasValue ? (double)order.PaidAmount.Value : -1,
                PaymentMethod = order.PaymentMethod ?? -1
            };

            if (!string.IsNullOrEmpty(order.Notes))
            {
                summary.Notes = order.Notes;
            }

            reply.Orders.Add(summary);
        }

        return reply;
    }
}