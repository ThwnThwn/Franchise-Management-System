using Ardalis.Result;
using FluentAssertions;
using Moq;
using SalesService.Application.AppExceptions;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Dtos.Inventory;
using SalesService.Application.Features.Orders.Commands.Create;
using SalesService.Application.Interfaces;
using SalesService.Application.Interfaces.CartItems;
using SalesService.Application.Interfaces.Franchise;
using SalesService.Application.Interfaces.Geocoding;
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

namespace SalesService.Application.Tests.Features.Orders;

public class CreateOrderHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<ICartItemRepository> _cartRepo = new();
    private readonly Mock<IProductServiceClient> _productClient = new();
    private readonly Mock<IInventoryServiceClient> _inventoryClient = new();
    private readonly Mock<IOutboxRepository> _outboxRepo = new();
    private readonly Mock<IOutboxSignal> _outboxSignal = new();
    private readonly Mock<IFranchiseServiceClient> _franchiseClient = new();
    private readonly Mock<IDistanceService> _distanceService = new();
    private readonly Mock<IGeocodingService> _geocodingService = new();
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTest()
    {
        _handler = new CreateOrderHandler(
            _orderRepo.Object,
            _cartRepo.Object,
            _productClient.Object,
            _inventoryClient.Object,
            _outboxRepo.Object,
            _outboxSignal.Object,
            _franchiseClient.Object,
            _distanceService.Object,
            _geocodingService.Object
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_CartEmpty()
    {
        var request = new CreateOrderRequestDto { Items = new List<CartItemDto>() };
        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("Cart is empty");
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_StoreIdMissing()
    {
        var request = new CreateOrderRequestDto 
        { 
            Items = new List<CartItemDto> { new CartItemDto { Sku = "SKU1", Quantity = 1 } },
            StoreId = Guid.Empty
        };
        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);
        result.Errors.First().Should().Contain("StoreId is required");
    }

    [Fact]
    public async Task Handle_Should_ThrowBusinessException_When_ScheduledTime_On_Pickup()
    {
        var request = new CreateOrderRequestDto 
        { 
            Items = new List<CartItemDto> { new CartItemDto { Sku = "SKU1", Quantity = 1 } },
            StoreId = Guid.NewGuid(),
            Type = OrderType.Pickup,
            ScheduledTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(2))
        };
        
        await _handler.Awaiting(x => x.Handle(new CreateOrderCommand(request), CancellationToken.None))
            .Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnShortage_When_StockNotEnough()
    {
        var storeId = Guid.NewGuid();
        var request = new CreateOrderRequestDto 
        { 
            Items = new List<CartItemDto> { new CartItemDto { Sku = "SKU1", Quantity = 10 } },
            StoreId = storeId,
            Type = OrderType.Pickup
        };

        var variants = new List<ProductVariantInfo> { new ProductVariantInfo { Sku = "SKU1", VariantId = Guid.NewGuid() } };
        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>())).ReturnsAsync(variants);
        
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>()))
            .ReturnsAsync(new CheckEnoughStockRes { IsEnough = false, ShortageItems = new List<AmountVariant> { new AmountVariant { VariantId = variants[0].VariantId, Amount = 5 } } });

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CheckEnoughStockRes.IsEnough.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Success_With_Delivery_And_Geocoding()
    {
        var storeId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var sku = "SKU1";
        var variantId = Guid.NewGuid();
        var request = new CreateOrderRequestDto 
        { 
            Items = new List<CartItemDto> { new CartItemDto { Sku = sku, Quantity = 2 } },
            StoreId = storeId,
            Type = OrderType.Delivery,
            Address = "Some Address",
            CustomerId = customerId
        };

        var variants = new List<ProductVariantInfo> { 
            new ProductVariantInfo { Sku = sku, VariantId = variantId, ProductId = Guid.NewGuid(), BasePrice = 50, ProductName = "Product 1" } 
        };
        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>())).ReturnsAsync(variants);
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>())).ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });
        
        _geocodingService.Setup(x => x.GeocodeAsync("Some Address")).ReturnsAsync(new CoordinateDto { Latitude = 10.0, Longitude = 106.0 });
        _franchiseClient.Setup(x => x.GetStoreDetailAsync(storeId)).ReturnsAsync(new StoreDetailInfo { Latitude = 10.05, Longitude = 106.05 });
        _distanceService.Setup(x => x.CalculateDistanceAsync(10.05, 106.05, 10.0, 106.0)).ReturnsAsync(7.0); // > 5km tier

        _orderRepo.Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ShippingFee.Should().Be(39000m); 
        _cartRepo.Verify(x => x.RemoveItemsAsync(customerId, It.IsAny<List<string>>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxSignal.Verify(x => x.Pulse(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Success_With_Delivery_2km_Tier()
    {
        var storeId = Guid.NewGuid();
        var sku = "SKU1";
        var request = new CreateOrderRequestDto 
        { 
            Items = new List<CartItemDto> { new CartItemDto { Sku = sku, Quantity = 1 } },
            StoreId = storeId,
            Type = OrderType.Delivery,
            Latitude = 10.0, Longitude = 106.0
        };

        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ProductVariantInfo> { new ProductVariantInfo { Sku = sku, BasePrice = 50 } });
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>())).ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });
        _franchiseClient.Setup(x => x.GetStoreDetailAsync(storeId)).ReturnsAsync(new StoreDetailInfo { Latitude = 10.01, Longitude = 106.01 });
        _distanceService.Setup(x => x.CalculateDistanceAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>())).ReturnsAsync(1.5); 

        _orderRepo.Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order o, CancellationToken ct) => o);

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);
        result.Value.ShippingFee.Should().Be(15000m);
    }

    [Fact]
    public async Task Handle_Should_Success_With_Delivery_5km_Tier()
    {
        var storeId = Guid.NewGuid();
        var sku = "SKU1";
        var request = new CreateOrderRequestDto { Items = new List<CartItemDto> { new CartItemDto { Sku = sku, Quantity = 1 } }, StoreId = storeId, Type = OrderType.Delivery, Latitude = 10.0, Longitude = 106.0 };

        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ProductVariantInfo> { new ProductVariantInfo { Sku = sku, BasePrice = 50 } });
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>())).ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });
        _franchiseClient.Setup(x => x.GetStoreDetailAsync(storeId)).ReturnsAsync(new StoreDetailInfo { Latitude = 10.01, Longitude = 106.01 });
        _distanceService.Setup(x => x.CalculateDistanceAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>())).ReturnsAsync(4.0); 

        _orderRepo.Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order o, CancellationToken ct) => o);

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);
        result.Value.ShippingFee.Should().Be(15000m + 2 * 4000m); // 2km -> 4km is 2km extra. 15k + 2*4k = 23k.
    }

    [Fact]
    public async Task Handle_Should_Throw_When_ScheduledTime_TooEarly()
    {
        var request = new CreateOrderRequestDto 
        { 
            Items = new List<CartItemDto> { new CartItemDto { Sku = "SKU1", Quantity = 1 } },
            StoreId = Guid.NewGuid(),
            Type = OrderType.Delivery,
            ScheduledTime = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(10)) 
        };
        
        await _handler.Awaiting(x => x.Handle(new CreateOrderCommand(request), CancellationToken.None))
            .Should().ThrowAsync<BusinessException>().WithMessage("Scheduled time must be at least 30 minutes in the future.");
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_GeocodeFails()
    {
        var request = new CreateOrderRequestDto 
        { 
            Items = new List<CartItemDto> { new CartItemDto { Sku = "SKU1", Quantity = 1 } },
            StoreId = Guid.NewGuid(),
            Type = OrderType.Delivery,
            Address = "Unknown Address"
        };
        
        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ProductVariantInfo> { new ProductVariantInfo { Sku = "SKU1", BasePrice = 10 } });
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>())).ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });
        _geocodingService.Setup(x => x.GeocodeAsync(It.IsAny<string>())).ReturnsAsync((CoordinateDto?)null);

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("Could not determine location");
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_Variant_Missing()
    {
        var request = new CreateOrderRequestDto 
        { 
            Items = new List<CartItemDto> { new CartItemDto { Sku = "SKU1", Quantity = 1 }, new CartItemDto { Sku = "MISSING", Quantity = 1 } },
            StoreId = Guid.NewGuid(),
            Type = OrderType.Pickup
        };
        
        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ProductVariantInfo> { new ProductVariantInfo { Sku = "SKU1" } });

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("variants not found");
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_PriceInvalid()
    {
        var request = new CreateOrderRequestDto 
        { 
            Items = new List<CartItemDto> { new CartItemDto { Sku = "SKU1", Quantity = 1 } },
            StoreId = Guid.NewGuid(),
            Type = OrderType.Pickup
        };
        
        var variants = new List<ProductVariantInfo> { new ProductVariantInfo { Sku = "SKU1", BasePrice = -10 } };
        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>())).ReturnsAsync(variants);
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>())).ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("invalid price");
    }
}
