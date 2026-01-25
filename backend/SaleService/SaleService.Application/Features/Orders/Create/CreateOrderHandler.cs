using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using MediatR;
using SaleService.Application.Features.Orders.Create;
using SaleService.Application.Interfaces;
using SaleService.Domain.Entities;
using SaleService.Domain.Enums;

namespace SaleService.Application.Features.Orders.Create;

public sealed class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductServiceClient _productServiceClient;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IProductServiceClient productServiceClient)
    {
        _orderRepository = orderRepository;
        _productServiceClient = productServiceClient;
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

        var orderCode = $"ORD-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

        // Gọi ProductService để lấy thông tin sản phẩm từ SKU
        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();
        var now = DateTime.UtcNow;

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
                OrderId = Guid.NewGuid(), // Sẽ set sau khi tạo Order
                ProductId = productVariant.ProductId,
                VariantId = productVariant.VariantId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                ProductName = productVariant.ProductName,
                Sku = productVariant.Sku,
                ImageUrl = productVariant.ImageUrl,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            });
        }

        decimal tax = request.Request.Tax ?? 0;
        decimal totalAmount = subtotal + tax;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Code = orderCode,
            Subtotal = subtotal,
            Tax = tax,
            TotalAmount = totalAmount,
            PaymentMethod = request.Request.PaymentMethod.HasValue
                ? (PaymentMethod?)request.Request.PaymentMethod.Value
                : null,
            Status = OrderStatus.Pending,
            Notes = request.Request.Notes,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        // Set OrderId cho các OrderItems
        foreach (var orderItem in orderItems)
        {
            orderItem.OrderId = order.Id;
            order.OrderItems.Add(orderItem);
        }

        var createdOrder = await _orderRepository.CreateOrderAsync(order);

        return Result.Success(new CreateOrderResponseDto
        {
            OrderId = createdOrder.Id,
            OrderCode = createdOrder.Code,
            TotalAmount = createdOrder.TotalAmount
        });
    }
}
