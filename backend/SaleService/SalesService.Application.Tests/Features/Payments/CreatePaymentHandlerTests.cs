using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using SalesService.Application.AppExceptions;
using SalesService.Application.ClientInterface.Engagement;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Dtos.Inventory;
using SalesService.Application.Dtos.Voucher;
using SalesService.Application.Features.Payments.CreatePayment;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Signals;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Constants;

namespace SalesService.Application.Tests.Features.Payments;

public class CreatePaymentHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo;
    private readonly Mock<IPaymentRepository> _paymentRepo;
    private readonly Mock<IConfiguration> _config;
    private readonly Mock<IOutboxRepository> _outboxRepo;
    private readonly Mock<IOutboxSignal> _outboxSignal;
    private readonly Mock<IEngagementServiceClient> _engagementClient;
    private readonly Mock<IProductServiceClient> _productClient;
    private readonly Mock<IInventoryServiceClient> _inventoryClient;
    private readonly Mock<IIdentityServiceClient> _identityClient;
    private readonly Mock<IWalletRepository> _walletRepo;
    private readonly Mock<IWalletTransactionRepository> _walletTxnRepo;
    private readonly CreatePaymentHandler _handler;

    public CreatePaymentHandlerTests()
    {
        _orderRepo = new Mock<IOrderRepository>();
        _paymentRepo = new Mock<IPaymentRepository>();
        _config = new Mock<IConfiguration>();
        _outboxRepo = new Mock<IOutboxRepository>();
        _outboxSignal = new Mock<IOutboxSignal>();
        _engagementClient = new Mock<IEngagementServiceClient>();
        _productClient = new Mock<IProductServiceClient>();
        _inventoryClient = new Mock<IInventoryServiceClient>();
        _identityClient = new Mock<IIdentityServiceClient>();
        _walletRepo = new Mock<IWalletRepository>();
        _walletTxnRepo = new Mock<IWalletTransactionRepository>();

        _handler = new CreatePaymentHandler(
            _orderRepo.Object,
            _paymentRepo.Object,
            _config.Object,
            _outboxRepo.Object,
            _outboxSignal.Object,
            _engagementClient.Object,
            _productClient.Object,
            _inventoryClient.Object,
            _identityClient.Object,
            _walletRepo.Object,
            _walletTxnRepo.Object
        );
    }

    [Fact]
    public async Task Handle_WhenOrderIdIsEmpty_ReturnsInvalid()
    {
        var request = new CreatePaymentReq { OrderId = Guid.Empty };
        var result = await _handler.Handle(new CreatePaymentCommand(request), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.First().Identifier.Should().Be("OrderId");
    }

    [Fact]
    public async Task Handle_WhenOrderNotFoundOrDeleted_ReturnsNotFound()
    {
        var orderId = Guid.NewGuid();
        var request = new CreatePaymentReq { OrderId = orderId };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _handler.Handle(new CreatePaymentCommand(request), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenOrderIsCompleted_ReturnsError()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Completed };
        var request = new CreatePaymentReq { OrderId = orderId };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new CreatePaymentCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("đã hoàn thành");
    }

    [Fact]
    public async Task Handle_WhenPaymentAlreadyPaid_ReturnsError()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.PaymentPending };
        var request = new CreatePaymentReq { OrderId = orderId };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _paymentRepo.Setup(x => x.GetNewestByOrderIdAndStatusAsync(orderId, PaymentStatus.Paid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Payment());

        var result = await _handler.Handle(new CreatePaymentCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("đã được thanh toán thành công");
    }

    [Fact]
    public async Task Handle_WhenVNPayAmountUnder10k_ReturnsError()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            StoreId = storeId,
            Subtotal = 50000m,
            ShippingFee = 0,
            OrderItems = [new() { Sku = "CF-ARABICA", Quantity = 1, UnitPrice = 50000m }],
            OrderSaleItems = []
        };

        var request = new CreatePaymentReq
        {
            OrderId = orderId,
            PaymentMethod = PaymentMethod.VNPay,
            Wallet = 46000m // TỔNG: 55k. Trừ ví 46k -> Còn lại VNPay 9k -> < 10k => Chắc chắn văng lỗi
        };

        var userWallet = new Wallet { Id = Guid.NewGuid(), Balance = 100000m };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _walletRepo.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(userWallet);
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>()))
            .ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });

        var result = await _handler.Handle(new CreatePaymentCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("phải từ 10.000 VNĐ trở lên");
    }

    [Fact]
    public async Task Handle_WhenWalletBalanceInsufficient_ThrowsBusinessException()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            Subtotal = 35000m,
            OrderItems = [],
            OrderSaleItems = []
        };

        var request = new CreatePaymentReq
        {
            OrderId = orderId,
            PaymentMethod = PaymentMethod.Cash,
            Wallet = 50000m
        };

        var userWallet = new Wallet { Id = Guid.NewGuid(), Balance = 10000m }; // Có 10k đòi xài 50k

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _walletRepo.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(userWallet);

        var action = async () => await _handler.Handle(new CreatePaymentCommand(request), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessException>().WithMessage("Số dư ví không đủ.");
    }

    [Fact]
    public async Task Handle_WhenInventoryShortageAtFinalReserve_ThrowsBusinessException()
    {
        var orderId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            StoreId = storeId,
            Subtotal = 30000m,
            OrderItems = [new() { Sku = "CAFE-DEN", Quantity = 1 }],
            OrderSaleItems = []
        };

        var request = new CreatePaymentReq { OrderId = orderId, PaymentMethod = PaymentMethod.Cash };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>()))
            .ReturnsAsync(new CheckEnoughStockRes { IsEnough = false });

        var action = async () => await _handler.Handle(new CreatePaymentCommand(request), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessException>().WithMessage("Không đủ số lượng hàng quà tặng trong kho!");
    }

    [Fact]
    public async Task Handle_WhenGuestCashPayment_Successful_GeneratesCashFlowEvents()
    {
        var orderId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            Code = "ORD-GUEST",
            StoreId = storeId,
            Subtotal = 40000m,
            ShippingFee = 15000m,
            OrderItems = [new() { Sku = "HONG-TRA", Quantity = 1 }],
            OrderSaleItems = []
        };

        var request = new CreatePaymentReq { OrderId = orderId, PaymentMethod = PaymentMethod.Cash };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>()))
            .ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });

        var result = await _handler.Handle(new CreatePaymentCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Url.Should().BeEmpty();

        order.Status.Should().Be(OrderStatus.ConfirmationPending);
        order.PaidAmount.Should().Be(60500m);

        _outboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m => m.Type == KafkaTopics.OrderNotifications)), Times.Once);
        _outboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m => m.Type == KafkaTopics.OrderPaid)), Times.Once);
        _outboxSignal.Verify(x => x.Pulse(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedUserWithGiftVoucherAndVNPay_GeneratesVNPayUrl()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            Code = "ORD-MATCHA",
            CustomerId = customerId,
            StoreId = storeId,
            Subtotal = 55000m,
            ShippingFee = 0,
            OrderItems = [new() { Sku = "MATCHA-LATTE", Quantity = 1 }],
            OrderSaleItems = []
        };

        var request = new CreatePaymentReq
        {
            OrderId = orderId,
            PaymentMethod = PaymentMethod.VNPay,
            VoucherDiscountCode = "TANG-BANH",
            ReturnUrl = "https://coffee.com/return"
        };

        var voucher = new Voucher
        {
            Id = Guid.NewGuid(),
            Code = "TANG-BANH",
            Type = VoucherType.GIFT,
            MinOrderValue = 50000m,
            VoucherItems = [new() { Type = VoucherItemType.ITEM_GIFT, Sku = "BANH-CROISSANT", Quantity = 1 }]
        };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _engagementClient.Setup(x => x.GetMyVoucherAsync(customerId)).ReturnsAsync([voucher]);

        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync([new() { Sku = "BANH-CROISSANT", BasePrice = 0 }]);

        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>()))
            .ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });

        var vnpSection = new Mock<IConfigurationSection>();
        vnpSection.Setup(s => s["PaymentUrl"]).Returns("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");
        vnpSection.Setup(s => s["HashSecret"]).Returns("SECRET");
        vnpSection.Setup(s => s["TmnCode"]).Returns("TMN");
        vnpSection.Setup(s => s["ReturnUrl"]).Returns("https://api.coffee.com/vnpay-return");
        _config.Setup(c => c.GetSection("VNPay")).Returns(vnpSection.Object);

        var result = await _handler.Handle(new CreatePaymentCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Url.Should().Contain("sandbox.vnpayment.vn");
        order.OrderItems.Should().Contain(x => x.Sku == "BANH-CROISSANT");
        order.OrderSaleItems.Should().Contain(x => x.Name == "BANH-CROISSANT");
    }

    [Fact]
    public async Task Handle_WithExistingGiftSaleItems_CleansUpRamCorrectly()
    {
        var orderId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            StoreId = storeId,
            Subtotal = 50000m,
            OrderItems =
            [
                new() { Sku = "TRA-SUA", Quantity = 2 },
                new() { Sku = "TOPPING-TRAN-CHAU", Quantity = 1 }
            ],
            OrderSaleItems =
            [
                new() { Type = OrderSaleItemType.Voucher, Name = "TRA-SUA", Amount = 1 },
                new() { Type = OrderSaleItemType.Voucher, Name = "TOPPING-TRAN-CHAU", Amount = 1 }
            ]
        };

        var request = new CreatePaymentReq { OrderId = orderId, PaymentMethod = PaymentMethod.Cash };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>()))
            .ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });

        var result = await _handler.Handle(new CreatePaymentCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.First().Sku.Should().Be("TRA-SUA");
        order.OrderItems.First().Quantity.Should().Be(1);
        order.OrderSaleItems.Should().BeEmpty();
    }
}