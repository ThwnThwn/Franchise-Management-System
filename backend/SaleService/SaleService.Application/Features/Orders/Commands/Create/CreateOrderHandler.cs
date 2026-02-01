using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Events;
using SaleService.Application.ExternalServices;
using SalesService.Application.Dtos.Inventory;
using SalesService.Application.Signals;

namespace SalesService.Application.Features.Orders.Commands.Create;

public sealed class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly IInventoryServiceClient _inventoryServiceClient;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxSignal _outboxSignal;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IProductServiceClient productServiceClient,
        IInventoryServiceClient inventoryServiceClient,
        IOutboxRepository outboxRepository,
        IOutboxSignal outboxSignal,
        ILogger<CreateOrderHandler> logger)
    {
        _orderRepository = orderRepository;
        _productServiceClient = productServiceClient;
        _inventoryServiceClient = inventoryServiceClient;
        _outboxRepository = outboxRepository;
        _outboxSignal = outboxSignal;
        _logger = logger;
    }

    public async Task<Result<CreateOrderResponseDto>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Request.Items == null || !request.Request.Items.Any())
        {
            return Result.Error("Cart is empty. Please add items to cart.");
        }

        // Validate: Mỗi item phải có SKU
        foreach (var item in request.Request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Sku))
            {
                return Result.Error($"Item is missing SKU. Please provide SKU for each item.");
            }
        }

        // check storeId
        if (!request.Request.StoreId.HasValue || request.Request.StoreId == Guid.Empty)
        {
            return Result.Error("StoreId is required. Please select a store before placing an order.");
        }
        
        //kiểm tra xem còn đủ hàng trong kho không
        var skus = request.Request.Items.Select(i => i.Sku).ToList();

        var variantInfo = await _productServiceClient.GetListProductVariantsBySkusAsync(skus);

        if(variantInfo.Count != skus.Count)
        {
            return Result.Error("One or more product variants not found or inactive.");
        }

        IEnumerable<AmountVariant> listItemCheck = variantInfo.Select(v => new AmountVariant
        {
            VariantId = v.VariantId,
            Amount = request.Request.Items.First(i => i.Sku == v.Sku).Quantity
        });

        var checkStockReq = new CheckEnoughStockReq
        {
            StoreId = request.Request.StoreId.Value,
            CheckItems = listItemCheck
        };
        var checkRespone = await _inventoryServiceClient.CheckEnoughStockAsync(checkStockReq);
        if(!checkRespone.IsEnough)
        {
            var responeErrorObject = new CreateOrderResponseDto
            {
                OrderId = Guid.Empty,
                OrderCode = "failed",
                TotalAmount = 0,
                CheckEnoughStockRes = checkRespone
            };
            return Result.Success(responeErrorObject);
        }

        var orderCode = $"ORD-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

        // Gọi ProductService để lấy thông tin sản phẩm từ SKU
        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Request.Items)
        {
            // Gọi ProductService để lấy thông tin từ SKU
            var productVariant = await _productServiceClient.GetProductVariantBySkuAsync(item.Sku);

            if (productVariant == null)
            {
                return Result.Error($"Product variant with SKU '{item.Sku}' not found or inactive.");
            }

            if (!productVariant.BasePrice.HasValue || productVariant.BasePrice <= 0)
            {
                return Result.Error($"Product variant with SKU '{item.Sku}' has invalid price.");
            }

            decimal unitPrice = productVariant.BasePrice.Value;
            subtotal += unitPrice * item.Quantity;

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ProductId = productVariant.ProductId,
                VariantId = productVariant.VariantId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                ProductName = productVariant.ProductName,
                Sku = productVariant.Sku,
                ImageUrl = productVariant.ImageUrl,
                IsDeleted = false
            });
        }

        // tax 10%
        const decimal taxRate = 0.1m;
        decimal tax = subtotal * taxRate;
        decimal totalAmount = subtotal + tax;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Code = orderCode,
            Subtotal = subtotal,
            Tax = tax,
            TotalAmount = totalAmount,
            StoreId = request.Request.StoreId, // đã validate khác null ở trên
            PaymentMethod = null,
            Status = OrderStatus.Pending,
            Notes = request.Request.Notes,
            CustomerId = request.Request.CustomerId,
            IsDeleted = false
        };

        // Set OrderId cho các OrderItems
        foreach (var orderItem in orderItems)
        {
            orderItem.OrderId = order.Id;
            order.OrderItems.Add(orderItem);
        }

        // 1. Prepare Outbox Message (BEFORE saving order)
        if (order.CustomerId.HasValue)
        {
             // Create OrderCreatedEvent
             var orderEvent = new OrderCreatedEvent
             {
                 OrderId = order.Id,
                 CustomerId = order.CustomerId,
                 CustomerEmail = string.Empty, // Will be fetched by Consumer
                 OrderSummary = $"Order {order.Code} created with {order.OrderItems.Count} items",
                 TotalAmount = order.TotalAmount,
                 CreatedAt = DateTimeOffset.UtcNow
             };

             // Serialize to JSON
             var eventJson = System.Text.Json.JsonSerializer.Serialize(orderEvent);

             var outboxMessage = new OutboxMessage
             {
                 Id = Guid.NewGuid(),
                 Type = "OrderCreatedEvent",
                 Content = eventJson,
                 OccurredOn = DateTimeOffset.UtcNow,
                 ProcessedOn = null,
                 Error = null
             };

             // Add to Outbox Repository
             _outboxRepository.Add(outboxMessage);
             _logger.LogInformation("Added OrderCreatedEvent to Outbox for Order {OrderCode}", order.Code);
        }

        // 2. Save Order (and OutboxMessage) in one transaction (implicitly handled by CreateAsync -> SaveChanges)
        var createdOrder = await _orderRepository.CreateAsync(order, cancellationToken);
        //Fetch customer email from IdentityService
        // string? customerEmail = null;
        // if (createdOrder.CustomerId.HasValue)
        
        // Optimize: Pulse Signal
        if (order.CustomerId.HasValue)
        {
            _outboxSignal.Pulse();
        }


        // ✅ Trừ kho sau khi tạo đơn hàng thành công
        try 
        {
            var decreaseStockReq = new DecreaseStockByOrderReq
            {
                StoreId = request.Request.StoreId.Value,
                Items = listItemCheck.ToList()
            };
            await _inventoryServiceClient.DecreaseStockByOrder(decreaseStockReq);
            _logger.LogInformation("Stock decreased successfully for Order {OrderCode}", createdOrder.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrease stock for Order {OrderCode}", createdOrder.Code);
            // TODO: Cần cơ chế compensate transaction (Saga) hoặc retry
        }

        return Result.Success(new CreateOrderResponseDto
        {
            OrderId = createdOrder.Id,
            OrderCode = createdOrder.Code,
            TotalAmount = createdOrder.TotalAmount,
            CheckEnoughStockRes = {IsEnough = true, ShortageItems = {}}
        });
    }
}
