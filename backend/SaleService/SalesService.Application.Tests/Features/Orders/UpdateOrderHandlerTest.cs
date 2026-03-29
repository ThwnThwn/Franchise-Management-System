using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SalesService.Application.AppExceptions;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Dtos.Inventory;
using SalesService.Application.Features.Orders.Commands.Update;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Signals;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// To avoid ambiguity between Domain.Entities.OrderItem and Update.OrderItem
using DomainOrderItem = SalesService.Domain.Entities.OrderItem;
using UpdateOrderItem = SalesService.Application.Features.Orders.Commands.Update.OrderItem;

namespace SalesService.Application.Tests.Features.Orders;

public class UpdateOrderHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IProductServiceClient> _productClient = new();
    private readonly Mock<IInventoryServiceClient> _inventoryClient = new();
    private readonly Mock<IOutboxRepository> _outboxRepo = new();
    private readonly Mock<IOutboxSignal> _outboxSignal = new();
    private readonly Mock<ILogger<UpdateOrderHandler>> _logger = new();
    private readonly UpdateOrderHandler _handler;

    public UpdateOrderHandlerTest()
    {
        _handler = new UpdateOrderHandler(
            _orderRepo.Object,
            _productClient.Object,
            _inventoryClient.Object,
            _outboxRepo.Object,
            _outboxSignal.Object,
            _logger.Object
        );
    }

    [Fact]
    public async Task Handle_Should_ThrowBusinessException_When_ItemsEmpty()
    {
        var request = new UpdateOrderReq { Items = new List<UpdateOrderItem>() };
        await _handler.Awaiting(x => x.Handle(new UpdateOrderCommand(request), CancellationToken.None))
            .Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowBusinessException_When_OrderNotFound()
    {
        var request = new UpdateOrderReq { OrderId = Guid.NewGuid(), Items = new List<UpdateOrderItem> { new UpdateOrderItem { Sku = "SKU1", Quantity = 1 } } };
        _orderRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        await _handler.Awaiting(x => x.Handle(new UpdateOrderCommand(request), CancellationToken.None))
            .Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowBusinessException_When_PermissionDenied()
    {
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderReq { OrderId = orderId, UserId = Guid.NewGuid(), RoleSlug = "customer", Items = new List<UpdateOrderItem> { new UpdateOrderItem { Sku = "SKU1", Quantity = 1 } } };
        var order = new Order { Id = orderId, CustomerId = Guid.NewGuid() };
        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await _handler.Awaiting(x => x.Handle(new UpdateOrderCommand(request), CancellationToken.None))
            .Should().ThrowAsync<BusinessException>().WithMessage("Olny staff/admin or the owner can update order");
    }

    [Fact]
    public async Task Handle_Should_ThrowBusinessException_When_PreparationStarted()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateOrderReq { OrderId = orderId, UserId = userId, RoleSlug = "customer", Items = new List<UpdateOrderItem> { new UpdateOrderItem { Sku = "SKU1", Quantity = 1 } } };
        var order = new Order { Id = orderId, CustomerId = userId, Status = OrderStatus.Preparing };
        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await _handler.Awaiting(x => x.Handle(new UpdateOrderCommand(request), CancellationToken.None))
            .Should().ThrowAsync<BusinessException>().WithMessage("Order cannot be updated after preparation has started.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Variant_Missing()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateOrderReq { OrderId = orderId, UserId = userId, RoleSlug = "customer", Items = new List<UpdateOrderItem> { new UpdateOrderItem { Sku = "SKU1", Quantity = 1 } } };
        var order = new Order { Id = orderId, CustomerId = userId, Status = OrderStatus.PaymentPending };
        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<ProductVariantInfo>());

        await _handler.Awaiting(x => x.Handle(new UpdateOrderCommand(request), CancellationToken.None))
            .Should().ThrowAsync<BusinessException>().WithMessage("One or more product variants not found.");
    }

    [Fact]
    public async Task Handle_Should_ReturnShortage_When_StockNotEnough()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sku = "SKU1";
        var request = new UpdateOrderReq { OrderId = orderId, UserId = userId, RoleSlug = "customer", Items = new List<UpdateOrderItem> { new UpdateOrderItem { Sku = sku, Quantity = 10 } } };
        var order = new Order { Id = orderId, CustomerId = userId, Status = OrderStatus.PaymentPending, StoreId = Guid.NewGuid() };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ProductVariantInfo> { new ProductVariantInfo { Sku = sku, VariantId = Guid.NewGuid() } });
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>())).ReturnsAsync(new CheckEnoughStockRes { IsEnough = false });

        var result = await _handler.Handle(new UpdateOrderCommand(request), CancellationToken.None);

        result.CheckEnoughStockRes.IsEnough.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_RemoveOldItems_And_AddNewItems()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var oldSku = "OLD";
        var newSku = "NEW";
        var request = new UpdateOrderReq 
        { 
            OrderId = orderId, UserId = userId, RoleSlug = "customer", 
            Items = new List<UpdateOrderItem> { new UpdateOrderItem { Sku = newSku, Quantity = 5 } } 
        };
        var order = new Order { Id = orderId, CustomerId = userId, Status = OrderStatus.PaymentPending, StoreId = Guid.NewGuid() };
        order.OrderItems.Add(new DomainOrderItem { Sku = oldSku, Quantity = 1 });

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ProductVariantInfo> { new ProductVariantInfo { Sku = newSku, BasePrice = 100, VariantId = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "P2" } });
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>())).ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });

        await _handler.Handle(new UpdateOrderCommand(request), CancellationToken.None);

        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.First().Sku.Should().Be(newSku);
    }

    [Fact]
    public async Task Handle_Should_ThrowArgumentException_When_PriceNull()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sku = "SKU1";
        var request = new UpdateOrderReq { OrderId = orderId, UserId = userId, RoleSlug = "customer", Items = new List<UpdateOrderItem> { new UpdateOrderItem { Sku = sku, Quantity = 1 } } };
        var order = new Order { Id = orderId, CustomerId = userId, Status = OrderStatus.PaymentPending, StoreId = Guid.NewGuid() };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ProductVariantInfo> { new ProductVariantInfo { Sku = sku, BasePrice = null } });
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>())).ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });

        await _handler.Awaiting(x => x.Handle(new UpdateOrderCommand(request), CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_Should_Success_When_Valid()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sku = "SKU1";
        var request = new UpdateOrderReq 
        { 
            OrderId = orderId, 
            UserId = userId,
            RoleSlug = "customer",
            Items = new List<UpdateOrderItem> { new UpdateOrderItem { Sku = sku, Quantity = 2 } } 
        };
        
        var order = new Order 
        { 
            Id = orderId, CustomerId = userId, Status = OrderStatus.PaymentPending, StoreId = Guid.NewGuid(), ShippingFee = 15000 
        };
        order.OrderItems.Add(new DomainOrderItem { Sku = sku, Quantity = 1, UnitPrice = 100, ProductId = Guid.NewGuid(), VariantId = Guid.NewGuid(), ProductName = "P1" });

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ProductVariantInfo> { new ProductVariantInfo { Sku = sku, BasePrice = 100, VariantId = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "P1" } });
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>())).ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });

        var result = await _handler.Handle(new UpdateOrderCommand(request), CancellationToken.None);

        result.Should().NotBeNull();
        order.OrderItems.First(x => x.Sku == sku).Quantity.Should().Be(2);
        _orderRepo.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
