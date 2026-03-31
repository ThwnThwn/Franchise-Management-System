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
using SalesService.Application.Signals;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Tests.Features.Orders;

public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo;
    private readonly Mock<ICartItemRepository> _cartRepo;
    private readonly Mock<IProductServiceClient> _productClient;
    private readonly Mock<IInventoryServiceClient> _inventoryClient;
    private readonly Mock<IOutboxSignal> _outboxSignal;
    private readonly Mock<IFranchiseServiceClient> _franchiseClient;
    private readonly Mock<IDistanceService> _distanceService;
    private readonly Mock<IGeocodingService> _geocodingService;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _orderRepo = new Mock<IOrderRepository>();
        _cartRepo = new Mock<ICartItemRepository>();
        _productClient = new Mock<IProductServiceClient>();
        _inventoryClient = new Mock<IInventoryServiceClient>();
        _outboxSignal = new Mock<IOutboxSignal>();
        _franchiseClient = new Mock<IFranchiseServiceClient>();
        _distanceService = new Mock<IDistanceService>();
        _geocodingService = new Mock<IGeocodingService>();

        _handler = new CreateOrderHandler(
            _orderRepo.Object,
            _cartRepo.Object,
            _productClient.Object,
            _inventoryClient.Object,
            _outboxSignal.Object,
            _franchiseClient.Object,
            _distanceService.Object,
            _geocodingService.Object
        );
    }

    [Fact]
    public async Task Handle_WhenCartIsEmpty_ReturnsError()
    {
        var request = new CreateOrderRequestDto { Items = [] };
        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("Cart is empty");
    }

    [Fact]
    public async Task Handle_WhenItemSkuIsMissing_ReturnsError()
    {
        var request = new CreateOrderRequestDto
        {
            Items = [new() { Sku = "   ", Quantity = 2 }]
        };
        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("missing SKU");
    }

    [Fact]
    public async Task Handle_WhenStoreIdIsMissing_ReturnsError()
    {
        var request = new CreateOrderRequestDto
        {
            Items = [new() { Sku = "CF-ARABICA-01", Quantity = 1 }],
            StoreId = Guid.Empty
        };
        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("StoreId is required");
    }

    [Fact]
    public async Task Handle_WhenOrderTypeIsInvalid_ReturnsError()
    {
        var request = new CreateOrderRequestDto
        {
            Items = [new CartItemDto { Sku = "CF-ARABICA-01", Quantity = 1 }],
            StoreId = Guid.NewGuid(),
            Type = (OrderType)999
        };
        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("Invalid OrderType");
    }

    [Fact]
    public async Task Handle_WhenScheduledTimeUsedForPickup_ThrowsBusinessException()
    {
        var request = new CreateOrderRequestDto
        {
            Items = new List<CartItemDto> { new CartItemDto { Sku = "CF-ROBUSTA-DA", Quantity = 1 } },
            StoreId = Guid.NewGuid(),
            Type = OrderType.Pickup,
            ScheduledTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(2))
        };

        var action = async () => await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessException>()
            .WithMessage("ScheduledTime can only be set for type Delivery orders.");
    }

    [Fact]
    public async Task Handle_WhenScheduledTimeIsTooEarly_ThrowsBusinessException()
    {
        var request = new CreateOrderRequestDto
        {
            Items = new List<CartItemDto> { new CartItemDto { Sku = "MACCHIATO-01", Quantity = 1 } },
            StoreId = Guid.NewGuid(),
            Type = OrderType.Delivery,
            ScheduledTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddMinutes(10))
        };

        var action = async () => await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessException>()
            .WithMessage("Scheduled time must be at least 30 minutes in the future.");
    }

    [Fact]
    public async Task Handle_WhenProductVariantsNotFound_ReturnsError()
    {
        var request = new CreateOrderRequestDto
        {
            Items = new List<CartItemDto>
            {
                new CartItemDto { Sku = "CF-SUA-DA", Quantity = 1 },
                new CartItemDto { Sku = "MATCHA-LATTE", Quantity = 1 }
            },
            StoreId = Guid.NewGuid()
        };

        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ProductVariantInfo>
            {
                new ProductVariantInfo { Sku = "CF-SUA-DA" }
            });

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("variants not found or inactive");
    }

    [Fact]
    public async Task Handle_WhenStockIsNotEnough_ReturnsSuccessWithShortageInfo()
    {
        var variantId = Guid.NewGuid();
        var request = new CreateOrderRequestDto
        {
            Items = new List<CartItemDto> { new CartItemDto { Sku = "COLD-BREW-01", Quantity = 50 } },
            StoreId = Guid.NewGuid()
        };

        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ProductVariantInfo>
            {
                new ProductVariantInfo { Sku = "COLD-BREW-01", VariantId = variantId }
            });

        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>()))
            .ReturnsAsync(new CheckEnoughStockRes
            {
                IsEnough = false,
                ShortageItems = new List<AmountVariant> { new AmountVariant { VariantId = variantId, Amount = 40 } }
            });

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CheckEnoughStockRes.IsEnough.Should().BeFalse();
        result.Value.CheckEnoughStockRes.ShortageItems.First().Sku.Should().Be("COLD-BREW-01");
    }

    [Fact]
    public async Task Handle_WhenProductPriceIsInvalid_ReturnsError()
    {
        var request = new CreateOrderRequestDto
        {
            Items = new List<CartItemDto> { new CartItemDto { Sku = "BAC-XIU", Quantity = 1 } },
            StoreId = Guid.NewGuid()
        };

        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ProductVariantInfo>
            {
                new ProductVariantInfo { Sku = "BAC-XIU", BasePrice = 0 }
            });

        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>()))
            .ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("has invalid price");
    }

    [Fact]
    public async Task Handle_WhenDeliveryGeocodingFails_ReturnsError()
    {
        var request = new CreateOrderRequestDto
        {
            Items = new List<CartItemDto> { new CartItemDto { Sku = "AMERICANO", Quantity = 1 } },
            StoreId = Guid.NewGuid(),
            Type = OrderType.Delivery,
            Address = "Hẻm cụt không xác định"
        };

        SetupValidProductAndInventory("AMERICANO", 40000m);
        _geocodingService.Setup(x => x.GeocodeAsync(It.IsAny<string>())).ReturnsAsync((CoordinateDto?)null);

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("Could not determine location");
    }

    [Fact]
    public async Task Handle_WhenDeliveryNoLocationProvided_ReturnsError()
    {
        var request = new CreateOrderRequestDto
        {
            Items = new List<CartItemDto> { new CartItemDto { Sku = "AMERICANO", Quantity = 1 } },
            StoreId = Guid.NewGuid(),
            Type = OrderType.Delivery
        };

        SetupValidProductAndInventory("AMERICANO", 40000m);

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("Delivery coordinates or a valid address are required");
    }

    [Fact]
    public async Task Handle_WhenDeliveryDistanceIsUnder2Km_CalculatesCorrectShippingFee()
    {
        var storeId = Guid.NewGuid();
        var request = new CreateOrderRequestDto
        {
            Items = new List<CartItemDto> { new CartItemDto { Sku = "ESPRESSO", Quantity = 2 } },
            StoreId = storeId,
            Type = OrderType.Delivery,
            Address = "123 Nguyễn Huệ, Quận 1, TP.HCM"
        };

        SetupValidProductAndInventory("ESPRESSO", 35000m);

        _geocodingService.Setup(x => x.GeocodeAsync(It.IsAny<string>()))
            .ReturnsAsync(new CoordinateDto { Latitude = 10.7769, Longitude = 106.7009 });

        _franchiseClient.Setup(x => x.GetStoreDetailAsync(storeId))
            .ReturnsAsync(new StoreDetailInfo { Latitude = 10.7800, Longitude = 106.7000 });

        _distanceService.Setup(x => x.CalculateDistanceAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(1.5);

        _orderRepo.Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ShippingFee.Should().Be(15000m);
        result.Value.TotalAmount.Should().Be(93500m);
    }

    [Fact]
    public async Task Handle_WhenDeliveryDistanceIsUnder5Km_CalculatesCorrectShippingFee()
    {
        var storeId = Guid.NewGuid();
        var request = new CreateOrderRequestDto
        {
            Items = new List<CartItemDto> { new CartItemDto { Sku = "TRA-DAO-CAM-SA", Quantity = 1 } },
            StoreId = storeId,
            Type = OrderType.Delivery,
            Latitude = 10.7,
            Longitude = 106.7
        };

        SetupValidProductAndInventory("TRA-DAO-CAM-SA", 50000m);
        _franchiseClient.Setup(x => x.GetStoreDetailAsync(storeId)).ReturnsAsync(new StoreDetailInfo());

        _distanceService.Setup(x => x.CalculateDistanceAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(4.1);

        _orderRepo.Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ShippingFee.Should().Be(27000m);
    }

    [Fact]
    public async Task Handle_WhenDeliveryDistanceIsOver5Km_CalculatesCorrectShippingFee()
    {
        var storeId = Guid.NewGuid();
        var request = new CreateOrderRequestDto
        {
            Items = new List<CartItemDto> { new CartItemDto { Sku = "CAPPUCCINO", Quantity = 1 } },
            StoreId = storeId,
            Type = OrderType.Delivery,
            Latitude = 10.7,
            Longitude = 106.7
        };

        SetupValidProductAndInventory("CAPPUCCINO", 55000m);
        _franchiseClient.Setup(x => x.GetStoreDetailAsync(storeId)).ReturnsAsync(new StoreDetailInfo());

        _distanceService.Setup(x => x.CalculateDistanceAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(7.2);

        _orderRepo.Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ShippingFee.Should().Be(45000m);
    }

    [Fact]
    public async Task Handle_WhenGuestOrderIsSuccessful_DoesNotPulseSignal()
    {
        var request = new CreateOrderRequestDto
        {
            Items = new List<CartItemDto> { new CartItemDto { Sku = "CA-PHE-SUA", Quantity = 1 } },
            StoreId = Guid.NewGuid(),
            Type = null
        };

        SetupValidProductAndInventory("CA-PHE-SUA", 29000m);
        _orderRepo.Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(OrderType.Pickup);
        _outboxSignal.Verify(x => x.Pulse(), Times.Never);
        _cartRepo.Verify(x => x.RemoveItemsAsync(It.IsAny<Guid?>(), It.IsAny<List<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedUserOrderIsSuccessful_PulsesSignal()
    {
        var request = new CreateOrderRequestDto
        {
            Items = new List<CartItemDto> { new CartItemDto { Sku = "MOCHA-CARAMEL", Quantity = 1 } },
            StoreId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid()
        };

        SetupValidProductAndInventory("MOCHA-CARAMEL", 60000m);
        _orderRepo.Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        var result = await _handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _outboxSignal.Verify(x => x.Pulse(), Times.Once);
    }

    private void SetupValidProductAndInventory(string sku, decimal price)
    {
        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ProductVariantInfo>
            {
                new ProductVariantInfo { Sku = sku, BasePrice = price, ProductId = Guid.NewGuid(), VariantId = Guid.NewGuid(), ProductName = "Test F&B Product" }
            });

        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>()))
            .ReturnsAsync(new CheckEnoughStockRes { IsEnough = true, ShortageItems = new List<AmountVariant>() });
    }
}