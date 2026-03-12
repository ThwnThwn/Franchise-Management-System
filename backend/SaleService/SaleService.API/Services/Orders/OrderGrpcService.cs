using AutoMapper;
using DocumentFormat.OpenXml.Office2016.Excel;
using Google.Protobuf;
using Grpc.Core;
using MediatR;
using Org.BouncyCastle.Asn1.Ocsp;
using SalesService.Application.Features.Orders.Commands.Create;
using SalesService.Application.Features.Orders.Commands.AssignStaff;
using SalesService.Application.Features.Orders.Commands.PayWithWallet;
using SalesService.Application.Features.Orders.Commands.Update;
using SalesService.Application.Features.Orders.Commands.UpdateOrderStatus;
using SalesService.Application.Features.Orders.Queries.GetById;
using SalesService.Application.Features.Orders.Queries.GetByIds;
using SalesService.Application.Features.Orders.Queries.GetMyOrders;
using SalesService.Application.Features.Orders.Queries.GetOrders;
using SalesService.Application.Features.Orders.Queries.GetOrderVolume;
using SalesService.Application.Features.Reports.Queries.CustomerPurchaseBehavior;
using SalesService.Application.Features.Reports.Queries.GetProductPerformance;
using SalesService.Application.Features.Reports.Queries.GetRevenueReport;
using SalesService.Application.Features.Reports.Queries.GetSalesTrend;
using SalesService.Application.Features.Reports.Queries.ProductPerformance;
using SalesService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SalesService.Application.Features.Orders.Queries.GetOrderTrackings;
using Microsoft.Extensions.Logging;

namespace SalesService.API.Services.Orders;

public class OrderGrpcService : OrderGrpc.OrderGrpcBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderGrpcService> _logger;

    public OrderGrpcService(IMediator mediator, IMapper mapper, ILogger<OrderGrpcService> logger)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }
    /// <summary>
    /// Tạo đơn hàng từ giỏ hàng
    /// </summary>
    /// <param name="request">The request received from the client.</param>
    /// <param name="context">The context of the server-side call handler being invoked.</param>
    /// <returns>
    /// The response to send back to the client (wrapped by a task).
    /// </returns>
    /// <exception cref="RpcException"></exception>
    /// <exception cref="Status">
    /// Missing x-user-id in request headers
    /// or
    /// </exception>

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
        var targetType = request.HasType ? (SalesService.Domain.Enums.OrderType)(int)request.Type : SalesService.Domain.Enums.OrderType.Pickup;

        _logger.LogInformation("CreateOrder request received: StoreId={StoreId}, RawType={RawType}, TargetType={TargetType}, Address={Address}, HasLat={HasLat}, HasLong={HasLong}",
            request.StoreId, (int)request.Type, targetType, request.Address, request.HasLatitude, request.HasLongitude);

        var createOrderRequest = new CreateOrderRequestDto
        {
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes,
            CustomerId = customerId,
            StoreId = string.IsNullOrWhiteSpace(request.StoreId) ? null : Guid.Parse(request.StoreId),
            Type = targetType,
            ScheduledTime = string.IsNullOrWhiteSpace(request.ScheduledTime) ? null : TimeOnly.Parse(request.ScheduledTime),
            Items = request.Items.Select(item => new CartItemDto
            {
                Sku = item.Sku,
                Quantity = item.Quantity
            }).ToList(),
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address,
            Latitude = request.HasLatitude ? request.Latitude : null,
            Longitude = request.HasLongitude ? request.Longitude : null
        };

        var command = new CreateOrderCommand(createOrderRequest);
        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? "Failed to create order";
            throw new RpcException(new Status(StatusCode.Internal, errorMessage));
        }

        var reply = new OrderReply
        {
            OrderId = result.Value.OrderId.ToString(),
            StoreId = result.Value.StoreId.ToString(),
            OrderCode = result.Value.OrderCode ?? string.Empty, // Tránh lỗi null cũ
            TotalAmount = (double)result.Value.TotalAmount,
            CustomerId = customerId.ToString(),
            Type = (OrderType)result.Value.Type,
            CheckStockRespone = new CheckEnoughStockResponse
            {
                IsEnough = result.Value.CheckEnoughStockRes.IsEnough
            },
            ScheduledTime = result.Value.ScheduledTime?.ToString("O") ?? string.Empty,
            ShippingFee = (double)result.Value.ShippingFee,
            Address = result.Value.Address ?? string.Empty,
            Latitude = result.Value.Latitude ?? 0,
            Longitude = result.Value.Longitude ?? 0,
            Distance = (double)(result.Value.Distance ?? 0)
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
                    Sku = i.Sku,
                    MaxFeasibleQuantity = i.Amount
                })
            );
        }
        reply.Items.AddRange(
            result.Value.Items.Select(i => new OrderItemReply
            {
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = (double)i.UnitPrice
            })
            );
        return reply;
    }
    /// <summary>
    /// Lấy thông tin đơn hàng theo ID
    /// </summary>
    /// <param name="request">The request received from the client.</param>
    /// <param name="context">The context of the server-side call handler being invoked.</param>
    /// <returns>
    /// The response to send back to the client (wrapped by a task).
    /// </returns>
    /// <exception cref="RpcException"></exception>
    /// <exception cref="Status">
    /// Invalid order id
    /// or
    /// </exception>
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
            Tax = dto.Tax.HasValue ? (double)dto.Tax : -1,
            PaidAmount = dto.PaidAmount.HasValue ? (double)dto.PaidAmount.Value : -1,
            PaymentMethod = dto.PaymentMethod ?? -1,
            Status = dto.Status,
            PaidAt = dto.PaidAt.ToString("O"),
            Notes = dto.Notes ?? string.Empty,
            CreatedAt = dto.CreatedAt.ToString("O"),
            UpdatedAt = dto.UpdatedAt.ToString("O"),
            CustomerId = dto.CustomerId.ToString(),
            StoreId = dto.StoreId?.ToString() ?? string.Empty,
            Type = (OrderType)dto.Type,
            ScheduledTime = dto.ScheduledTime?.ToString("O") ?? string.Empty,
            ShippingFee = (double)dto.ShippingFee,
            Address = dto.Address ?? string.Empty,
            Latitude = dto.Latitude ?? 0,
            Longitude = dto.Longitude ?? 0
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

        orderMessage.SaleItems.AddRange(dto.SaleItems.Select(i => new OrderSaleItemReply
        {
            Id = i.Id.ToString(),
            Amount = (double)i.Amount,
            Type = (OrderSaleItemType)i.Type,
            ReferenceId = i.ReferenceId.ToString(),
            Name = i.Name

        }));
        return new GetOrderByIdReply
        {
            Order = orderMessage
        };
    }
    /// <summary>
    /// Lấy danh sách đơn hàng của khách hàng hiện tại
    /// </summary>
    /// <param name="request">The request received from the client.</param>
    /// <param name="context">The context of the server-side call handler being invoked.</param>
    /// <returns>
    /// The response to send back to the client (wrapped by a task).
    /// </returns>
    /// <exception cref="RpcException"></exception>
    /// <exception cref="Status">Missing x-user-id in request headers</exception>
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
                StoreId = order.StoreId?.ToString() ?? "",
                Notes = order.Notes ?? string.Empty,

                //dùng -1 cho NULL (chưa thanh toán)
                Tax = order.Tax.HasValue ? (double)order.Tax.Value : -1,
                PaidAmount = order.PaidAmount.HasValue ? (double)order.PaidAmount.Value : -1,
                PaymentMethod = order.PaymentMethod ?? -1,
                Type = (OrderType)order.OrderType,
                ScheduledTime = order.ScheduledTime?.ToString("O") ?? string.Empty,
                ShippingFee = (double)order.ShippingFee,
                Address = order.Address ?? string.Empty,
                Latitude = order.Latitude ?? 0,
                Longitude = order.Longitude ?? 0
            };

            if (!string.IsNullOrEmpty(order.Notes))
            {
                orderSummary.Notes = order.Notes;
            }

            reply.Orders.Add(orderSummary);
        }

        return reply;
    }

    public override async Task<UpdateOrderStatusReply> UpdateOrderStatus(UpdateOrderStatusRequest request, ServerCallContext context)
    {
        var roleSlug = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-role-slug", StringComparison.OrdinalIgnoreCase));
        if (roleSlug == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-role-slug in request headers"));
        }
        //Validate orderId
        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid OrderId"));
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
                Status = (Domain.Enums.OrderStatus)(int)request.Status,
                RoleSlug = roleSlug.ToString()
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
            StoreId = result.StoreId.ToString(),
            TotalAmount = (double)(result.TotalAmount ?? 0),
            CustomerId = result.CustomerId.ToString(),
            CheckStockRespone = new CheckEnoughStockResponse
            {
                IsEnough = result.CheckEnoughStockRes.IsEnough
            },
            ShippingFee = (double)(result.ShippingFee ?? 0)
        };

        // Add shortage items if any
        if (result.CheckEnoughStockRes.ShortageItems != null)
        {
            reply.CheckStockRespone.ShortageItems.AddRange(
                result.CheckEnoughStockRes.ShortageItems.Select(x =>
                    new ShortageItem
                    {
                        VariantId = x.VariantId.ToString(),
                        Sku = x.Sku,
                        MaxFeasibleQuantity = x.Amount
                    })
            );
        }
        reply.Items.AddRange(
            result.Items.Select(i => new OrderItemReply
            {
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = (double)i.UnitPrice
            })
            );
        return reply;
    }

    /// <summary>
    /// Lấy danh sách đơn hàng với các bộ lọc: theo cửa hàng, theo mã đơn hàng, theo khoảng thời gian, theo trạng thái, theo phương thức thanh toán
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="RpcException"></exception>
    public override async Task<GetOrdersReply> GetOrders(GetOrdersRequest request, ServerCallContext context)
    {
        var storeId = string.IsNullOrEmpty(request.StoreId) ? (Guid?)null : Guid.Parse(request.StoreId);
        var fromDate = string.IsNullOrEmpty(request.FromDate) ? (DateOnly?)null : DateOnly.Parse(request.FromDate);
        var toDate = string.IsNullOrEmpty(request.ToDate) ? (DateOnly?)null : DateOnly.Parse(request.ToDate);
        var status = request.HasStatus ? (Domain.Enums.OrderStatus?)request.Status : null;
        var paymentMethod = request.HasPaymentMethod ? (Domain.Enums.PaymentMethod?)request.PaymentMethod : null;
        var type = request.HasType ? (Domain.Enums.OrderType?)request.Type : null;
        var query = new GetOrdersQuery(
            storeId,
            request.Code,
            fromDate,
            toDate,
            status,
            paymentMethod,
            type,
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
            TotalCount = Convert.ToInt32(result.Value.PageResult.TotalCount),
            TotalPages = result.Value.PageResult.TotalPages
        };

        foreach (var order in result.Value.PageResult.Items)
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
                StoreId = order.StoreId?.ToString() ?? "",
                Notes = order.Notes ?? string.Empty,
                Tax = order.Tax.HasValue ? (double)order.Tax.Value : -1,
                PaidAmount = order.PaidAmount.HasValue ? (double)order.PaidAmount.Value : -1,
                PaymentMethod = order.PaymentMethod ?? -1,
                Type = (OrderType)order.OrderType,
                ScheduledTime = order.ScheduledTime?.ToString("O") ?? string.Empty,
                ShippingFee = (double)order.ShippingFee,
                Address = order.Address ?? string.Empty,
                Latitude = order.Latitude ?? 0,
                Longitude = order.Longitude ?? 0
            };

            if (!string.IsNullOrEmpty(order.Notes))
            {
                summary.Notes = order.Notes;
            }

            reply.Orders.Add(summary);
        }

        return reply;
    }

    /// <summary>
    /// Báo cáo doanh thu theo khoảng thời gian và danh mục sản phẩm (nếu có)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="RpcException"></exception>
    public override async Task<GetRevenueReportReply> GetRevenueReport(
        GetRevenueReportRequest request,
        ServerCallContext context)
    {
        DateOnly? fromDate = string.IsNullOrEmpty(request.FromDate)
            ? (DateOnly?)null
            : DateOnly.Parse(request.FromDate);

        DateOnly? toDate = string.IsNullOrEmpty(request.ToDate)
            ? (DateOnly?)null
            : DateOnly.Parse(request.ToDate);

        Guid? categoryId = string.IsNullOrEmpty(request.CategoryId)
            ? (Guid?)null
            : Guid.Parse(request.CategoryId);

        Guid? storeId = string.IsNullOrEmpty(request.StoreId)
            ? (Guid?)null
            : Guid.Parse(request.StoreId);
        var query = new GetRevenueReportQuery(fromDate, toDate, categoryId, storeId);
        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
        {
            throw new RpcException(new Status(
                StatusCode.Internal,
                result.Errors.FirstOrDefault() ?? "Failed to get revenue report"));
        }
        var report = result.Value;

        var reply = new GetRevenueReportReply
        {
            FromDate = result.Value.FromDate?.ToString("O") ?? string.Empty,
            ToDate = result.Value.ToDate?.ToString("O") ?? string.Empty,
            CategoryId = result.Value.CategoryId?.ToString() ?? string.Empty,
            OverallTotalRevenue = (double)result.Value.OverallTotalRevenue,
        };

        reply.Items.AddRange(result.Value.Items.Select(i => new RevenueItem
        {
            ProductName = i.ProductName,
            Sku = i.Sku,
            QuantitySold = i.QuantitySold,
            UnitPrice = (double)i.UnitPrice,
            TotalRevenue = (double)i.TotalRevenue,
            StoreId = i.StoreId?.ToString() ?? string.Empty,
        }));

        if (request.IncludeCsv)
        {
            var csvBytes = RevenueReportCsvExporter.Export(report);
            reply.CsvFile = new CsvFile
            {
                CsvRaw = ByteString.CopyFrom(csvBytes),
                CsvFileName = $"revenue-report-{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
            };
        }

        return reply;
    }

    public override async Task<GetOrderVolumeReply> GetOrderVolume(
    GetOrderVolumeRequest request,
    ServerCallContext context)
    {
        var fromDate = DateOnly.Parse(request.FromDate);
        var toDate = DateOnly.Parse(request.ToDate);

        Guid? storeId = string.IsNullOrEmpty(request.StoreId)
            ? null
            : Guid.Parse(request.StoreId);

        SalesService.Domain.Enums.OrderStatus? domainStatus = null;
        if (request.HasStatus)
        {
            domainStatus =
                (SalesService.Domain.Enums.OrderStatus)(int)request.Status;
        }

        var result = await _mediator.Send(
            new GetOrderVolumeQuery(
                new Application.Features.Orders.Queries.GetOrderVolume.GetOrderVolumeRequest
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    StoreId = storeId,
                    Status = domainStatus,
                    Include_csv = request.IncludeCsv
                }),
            context.CancellationToken);

        var reply = new GetOrderVolumeReply();

        foreach (var item in result.Orders)
        {
            var order = new OrderVolumeADay
            {
                Date = item.Date.ToString("yyyy-MM-dd"),
                TotalOrders = item.TotalOrders,
                CanceledOrder = item.CanceledOrders,
                CompletedOrder = item.CompletedOrders,
            };

            if (item.StoreId.HasValue)
            {
                order.StoreId = item.StoreId.Value.ToString();
            }
            foreach (var statusGroup in item.StatusCounts)
            {
                order.StatusCounts[statusGroup.Key.ToString()] = statusGroup.Value;
            }
            reply.Orders.Add(order);
        }
        if (result.CsvFile != null)
        {
            reply.CsvFile = new CsvFile
            {
                CsvRaw = ByteString.CopyFrom(result.CsvFile),
                CsvFileName = result.CsvFileName!
            };
        }
        return reply;
    }

    public override async Task<PayOrderWithWalletReply> PayOrderWithWallet(
    PayOrderWithWalletRequest request,
    ServerCallContext context)
    {
        // 1️⃣ Validate OrderId
        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid order_id"));
        }

        // 2️⃣ Lấy CustomerId từ metadata
        var userIdEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));

        if (userIdEntry == null)
        {
            throw new RpcException(new Status(
                StatusCode.Unauthenticated,
                "Missing x-user-id in request headers"));
        }

        var customerId = Guid.Parse(userIdEntry.Value);

        // 3️⃣ Send command
        var command = new PayOrderWithWalletCommand(orderId, customerId);
        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            throw new RpcException(new Status(
                StatusCode.Internal,
                result.Errors.FirstOrDefault() ?? "Failed to pay order"));
        }

        var dto = result.Value;

        // 4️⃣ Map sang gRPC reply
        return new PayOrderWithWalletReply
        {
            OrderId = dto.OrderId.ToString(),
            OrderCode = dto.OrderCode,

            CustomerWalletId = dto.CustomerWalletId.ToString(),
            ShopWalletId = dto.ShopWalletId.ToString(),

            Amount = (double)dto.Amount,

            CustomerBalanceBefore = (double)dto.CustomerBalanceBefore,
            CustomerBalanceAfter = (double)dto.CustomerBalanceAfter,

            ShopBalanceBefore = (double)dto.ShopBalanceBefore,
            ShopBalanceAfter = (double)dto.ShopBalanceAfter,

            CustomerTransactionId = dto.CustomerTransactionId.ToString(),
            ShopTransactionId = dto.ShopTransactionId.ToString(),

            CustomerTransactionCode = dto.CustomerTransactionCode,
            ShopTransactionCode = dto.ShopTransactionCode,

            PaidAt = dto.PaidAt.ToString("O")
        };
    }

    public override async Task<GetCountOrderByUserIdsResponse> GetCountOrderByUserIds(GetCountOrderByUserIdsRequest request, ServerCallContext context)
    {
        if (request.UserId == null || request.UserId.Count == 0)
            return new GetCountOrderByUserIdsResponse();

        // Convert string → Guid safely
        var userIds = request.UserId
            .Where(id => Guid.TryParse(id, out _))
            .Select(Guid.Parse)
            .ToList();

        var result = await _mediator.Send(
            new GetByUserIdsQuery
            {
                UserIds = userIds
            });

        var response = new GetCountOrderByUserIdsResponse();

        response.UserCountOrder.AddRange(
            result.Select(x => new CountOrderByUserId
            {
                UserId = x.UserId.ToString(),
                OrderCount = x.OrderCount
            }));

        return response;
    }

    public override async Task<GetProductPerformanceReply> GetProductPerformance(
       GetProductPerformanceRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.StoreId, out var storeId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid StoreId format"));

        DateTimeOffset fromDate;
        if (!DateTime.TryParse(request.FromDate, out var fromDt))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid from_date format: {request.FromDate}"));
        fromDate = new DateTimeOffset(fromDt, TimeSpan.FromHours(7));

        DateTimeOffset toDate;
        if (!DateTime.TryParse(request.ToDate, out var toDt))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid to_date format: {request.ToDate}"));
        toDate = new DateTimeOffset(toDt, TimeSpan.FromHours(7))
                     .AddDays(1).AddTicks(-1);

        Guid? categoryId = null;
        if (request.HasCategoryId && Guid.TryParse(request.CategoryId, out var parsedCategoryId))
            categoryId = parsedCategoryId;

        var query = new GetProductPerformanceQuery(
            StoreId: storeId,
            FromDate: fromDate,
            ToDate: toDate,
            CategoryId: categoryId,
            SortBy: request.HasSortBy ? request.SortBy : null,
            SortOrder: request.HasSortOrder ? request.SortOrder : null,
            PageIndex: request.PageIndex > 0 ? request.PageIndex : 1,
            PageSize: request.PageSize > 0 ? request.PageSize : 20
        );
        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
            throw new RpcException(new Status(StatusCode.Internal,
                string.Join(", ", result.Errors)));

        var reply = new GetProductPerformanceReply
        {
            TotalCount = result.Value.TotalPages,
            PageIndex = result.Value.Page,
            PageSize = result.Value.PageSize
        };

        reply.Items.AddRange(result.Value.Items.Select(item => new ProductPerformanceItem
        {
            ProductId = item.ProductId.ToString(),
            ProductName = item.ProductName,
            CategoryId = item.CategoryId.ToString(),
            CategoryName = item.CategoryName ?? string.Empty,
            UnitsSold = item.UnitsSold,
            Revenue = (double)item.Revenue,
            CancelledRate = (double)item.CancelledRate,
            TotalOrders = item.TotalOrders,
            TotalCancelled = item.TotalCancelled,
            Tag = item.Tag switch
            {
                Domain.Enums.PerformanceTag.Top => PerformanceTag.Top,
                Domain.Enums.PerformanceTag.Bottom => PerformanceTag.Bottom,
                _ => PerformanceTag.Normal
            }
        }));

        if (request.HasIncludeCsv && request.IncludeCsv)
        {
            var csvBytes = ProductPerformanceCsvExporter.Export(result.Value.Items.ToList());
            reply.CsvFile = new CsvFile
            {
                CsvRaw = Google.Protobuf.ByteString.CopyFrom(csvBytes),
                CsvFileName = $"product-performance-{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
            };
        }
        return reply;
    }

    public override async Task<GetSalesTrendReply> GetSalesTrend(GetSalesTrendRequest request, ServerCallContext context)
    {
        DateTimeOffset fromDate;
        if (!DateTimeOffset.TryParse(request.FromDate, out fromDate))
        {
            if (DateTime.TryParse(request.FromDate, out var dt))
                fromDate = new DateTimeOffset(dt, TimeSpan.FromHours(7));
            else
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid from_date format: {request.FromDate}"));
        }

        DateTimeOffset toDate;
        if (!DateTimeOffset.TryParse(request.ToDate, out toDate))
        {
            if (DateTime.TryParse(request.ToDate, out var dt))
                toDate = new DateTimeOffset(dt, TimeSpan.FromHours(7));
            else
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid to_date format: {request.ToDate}"));
        }

        var aggregation = (TimeAggregation)request.Aggregation;

        Guid? storeId = string.IsNullOrEmpty(request.StoreId) ? null : Guid.Parse(request.StoreId);
        Guid? categoryId = string.IsNullOrEmpty(request.CategoryId) ? null : Guid.Parse(request.CategoryId);

        var query = new GetSalesTrendQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            Aggregation = aggregation,
            StoreId = storeId,
            CategoryId = categoryId
        };

        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
        {
            throw new RpcException(new Status(StatusCode.Internal, result.Errors.FirstOrDefault() ?? "Failed to get sales trend"));
        }

        var reply = new GetSalesTrendReply
        {
            PeakPeriodLabel = result.Value.PeakPeriodLabel ?? string.Empty,
            SlowPeriodLabel = result.Value.SlowPeriodLabel ?? string.Empty
        };

        reply.DataPoints.AddRange(result.Value.DataPoints.Select(dp => new SalesTrendDataPoint
        {
            TimeLabel = dp.TimeLabel,
            OrderVolume = dp.OrderVolume,
            Revenue = (double)dp.Revenue
        }));

        if (request.IncludeCsv)
        {
            var csvBytes = SalesTrendCsvExporter.Export(result.Value);
            reply.CsvFile = new CsvFile
            {
                CsvRaw = ByteString.CopyFrom(csvBytes),
                CsvFileName = $"sales-trend-{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
            };
        }
        return reply;
    }

    public override async Task<GetCustomerOrderPatternReply> GetCustomerOrderPattern(
    GetCustomerOrderPatternRequest request,
    ServerCallContext context)
    {
        if (!Guid.TryParse(request.StoreId, out var storeId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid StoreId"));

        DateTimeOffset fromDate;
        if (!DateTime.TryParse(request.FromDate, out var fromDt))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid from_date format: {request.FromDate}"));
        fromDate = new DateTimeOffset(fromDt, TimeSpan.FromHours(7));

        DateTimeOffset toDate;
        if (!DateTime.TryParse(request.ToDate, out var toDt))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid to_date format: {request.ToDate}"));
        toDate = new DateTimeOffset(toDt, TimeSpan.FromHours(7))
                     .AddDays(1).AddTicks(-1);



        var query = new GetCustomerOrderPatternQuery(
            StoreId: storeId,
            FromDate: fromDate,
            ToDate: toDate,
            MinOrders: request.HasMinOrders ? request.MinOrders : null,
            MaxOrders: request.HasMaxOrders && request.MaxOrders > 0
                ? request.MaxOrders
                : null,
            SortBy: request.HasSortBy ? request.SortBy : null,
            SortOrder: request.HasSortOrder ? request.SortOrder : null,
            PageIndex: request.PageIndex > 0 ? request.PageIndex : 1,
            PageSize: request.PageSize > 0 ? request.PageSize : 20
        );

        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
            throw new RpcException(new Status(StatusCode.Internal,
                string.Join(", ", result.Errors)));

        var reply = new GetCustomerOrderPatternReply
        {
            TotalCount = (int)result.Value.TotalCount,
            PageIndex = result.Value.Page,
            PageSize = result.Value.PageSize
        };

        reply.Items.AddRange(result.Value.Items.Select(item => new CustomerOrderPatternItem
        {
            CustomerId = item.CustomerId.ToString(),
            FullName = item.FullName??"",
            Email = item.Email ?? "",
            Phone = item.Phone ?? "",
            TotalOrders = item.TotalOrders,
            AverageOrderValue = (double)item.AverageOrderValue,
            TotalSpent = (double)item.TotalSpent,
            LastOrderDate = item.LastOrderDate.ToString("O")
        }));

        if (request.HasIncludeCsv && request.IncludeCsv)
        {
            var csvBytes = CustomerOrderPatternCsvExporter.Export(result.Value.Items.ToList());
            reply.CsvFile = new CsvFile
            {
                CsvRaw = Google.Protobuf.ByteString.CopyFrom(csvBytes),
                CsvFileName = $"customer-order-pattern-{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
            };
        }
        return reply;
    }

    public override async Task<AssignStaffToDeliveryReply> AssignStaffToDelivery(
        AssignStaffToDeliveryRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid order_id format."));

        Guid? staffId = null;
        if (request.HasStaffId && !string.IsNullOrWhiteSpace(request.StaffId))
        {
            if (!Guid.TryParse(request.StaffId, out var parsedStaffId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid staff_id format."));
            staffId = parsedStaffId;
        }

        var command = new AssignStaffToDeliveryCommand(orderId, staffId);
        var result = await _mediator.Send(command, context.CancellationToken);

        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            throw new RpcException(new Status(StatusCode.NotFound, result.Errors.FirstOrDefault() ?? "Order not found."));

        if (!result.IsSuccess)
            throw new RpcException(new Status(StatusCode.InvalidArgument, result.Errors.FirstOrDefault() ?? "Failed to assign staff."));

        var dto = result.Value;
        return new AssignStaffToDeliveryReply
        {
            OrderId = dto.OrderId.ToString(),
            OrderCode = dto.OrderCode,
            DelivererId = dto.DelivererId?.ToString() ?? string.Empty,
            Message = dto.Message
        };
    }

    public override async Task<GetOrderTrackingsReply> GetOrderTrackings(
        GetOrderTrackingsRequest request,
        ServerCallContext context)
    {
        Guid? storeId = null;
        if (!string.IsNullOrWhiteSpace(request.StoreId))
        {
            if (Guid.TryParse(request.StoreId, out var parsedStoreId))
                storeId = parsedStoreId;
        }

        Guid? orderId = null;
        if (!string.IsNullOrWhiteSpace(request.OrderId))
        {
            if (Guid.TryParse(request.OrderId, out var parsedOrderId))
                orderId = parsedOrderId;
        }

        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;

        var query = new GetOrderTrackingsQuery(storeId, orderId, page, pageSize);
        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
            throw new RpcException(new Status(StatusCode.Internal, result.Errors.FirstOrDefault() ?? "Failed to get order trackings."));

        var response = new GetOrderTrackingsReply
        {
            TotalCount = result.Value.TotalCount
        };

        response.Trackings.AddRange(result.Value.Trackings.Select(t => new OrderTrackingMessage
        {
            Id = t.Id.ToString(),
            OrderId = t.OrderId.ToString(),
            OrderCode = t.OrderCode,
            Status = t.Status,
            Action = t.Action,
            Note = t.Note,
            CreatedAt = t.CreatedAt.ToString("O")
        }));

        return response;
    }
}