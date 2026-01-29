using Ardalis.Result;
using MediatR;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Events;
using SaleService.Application.ExternalServices;
using SalesService.Application.Dtos.Inventory;

namespace SalesService.Application.Features.Orders.Commands.Create;

public sealed class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IIdentityServiceClient _identityServiceClient;
    private readonly IInventoryServiceClient _inventoryServiceClient;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IProductServiceClient productServiceClient,
        IKafkaProducer kafkaProducer,
        IIdentityServiceClient identityServiceClient,
        IInventoryServiceClient inventoryServiceClient)
    {
        _orderRepository = orderRepository;
        _productServiceClient = productServiceClient;
        _kafkaProducer = kafkaProducer;
        _identityServiceClient = identityServiceClient;
        _inventoryServiceClient = inventoryServiceClient;
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

        IEnumerable<AmountVariant> listItemCheck = variantInfo.Select(v => new AmountVariant
        {
            VariantId = v.VariantId,
            Amount = request.Request.Items.First(i => i.Sku == v.Sku).Quantity
        });

        // IEnumerable<AmountVariant> listItemCheck = new List<AmountVariant>
        //{
        //    new AmountVariant
        //    {
        //        VariantId = Guid.Parse("F0000008-0000-0000-0000-0000000000F2"),
        //        Amount = 2
        //    }
        //};

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

        var createdOrder = await _orderRepository.CreateAsync(order, cancellationToken);

        // Fetch customer email from IdentityService
        //string? customerEmail = null;
        //if (createdOrder.CustomerId.HasValue)
        //{
        //    var user = await _identityServiceClient.GetUserByIdAsync(createdOrder.CustomerId.Value);
        //    customerEmail = user?.Email;
        //}

        //// Only publish event if we have customer email
        //if (!string.IsNullOrEmpty(customerEmail))
        //{
        //    await _kafkaProducer.PublishAsync("order-notifications", new OrderCreatedEvent
        //    {
        //        OrderId = createdOrder.Id,
        //        CustomerEmail = customerEmail,
        //        OrderSummary = $"Order {createdOrder.Code} created with {createdOrder.OrderItems.Count} items",
        //        TotalAmount = createdOrder.TotalAmount,
        //        CreatedAt = DateTimeOffset.UtcNow
        //    });
        //}

        return Result.Success(new CreateOrderResponseDto
        {
            OrderId = createdOrder.Id,
            OrderCode = createdOrder.Code,
            TotalAmount = createdOrder.TotalAmount,
            CheckEnoughStockRes = {IsEnough = true, ShortageItems = {}}
        });
    }
}