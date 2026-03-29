using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using SalesService.Application.ClientInterface.Engagement;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Features.Payments.CreatePayment;
using SalesService.Application.Dtos.Voucher;
using SalesService.Application.Dtos.Inventory;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Signals;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Infrastructure.Kafka;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SalesService.Application.Tests.Features.Payments;

public class CreatePaymentHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<IKafkaProducer> _kafkaProducer = new();
    private readonly Mock<IConfiguration> _config = new();
    private readonly Mock<IOutboxRepository> _outboxRepo = new();
    private readonly Mock<IOutboxSignal> _outboxSignal = new();
    private readonly Mock<IEngagementServiceClient> _engagementClient = new();
    private readonly Mock<IProductServiceClient> _productClient = new();
    private readonly Mock<IInventoryServiceClient> _inventoryClient = new();
    private readonly Mock<IIdentityServiceClient> _identityClient = new();
    private readonly Mock<IWalletRepository> _walletRepo = new();

    private readonly CreatePaymentHandler _handler;

    public CreatePaymentHandlerTests()
    {
        _handler = new CreatePaymentHandler(
            _orderRepo.Object,
            _paymentRepo.Object,
            _kafkaProducer.Object,
            _config.Object,
            _outboxRepo.Object,
            _outboxSignal.Object,
            _engagementClient.Object,
            _productClient.Object,
            _inventoryClient.Object,
            _identityClient.Object,
            _walletRepo.Object
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_OrderId_Empty()
    {
        var command = new CreatePaymentCommand(new CreatePaymentReq { OrderId = Guid.Empty });
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_Order_NotExist()
    {
        var orderId = Guid.NewGuid();
        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new CreatePaymentCommand(new CreatePaymentReq { OrderId = orderId });
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ProcessCashPayment_Successfully()
    {
        var order = new Order { Id = Guid.NewGuid(), Code = "ORD-001", Subtotal = 1000, ShippingFee = 50, Status = OrderStatus.PaymentPending };
        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _paymentRepo.Setup(x => x.GetNewestByOrderIdAndStatusAsync(order.Id, PaymentStatus.Paid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var request = new CreatePaymentReq { OrderId = order.Id, PaymentMethod = PaymentMethod.Cash };
        var command = new CreatePaymentCommand(request);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.ConfirmationPending);
        _paymentRepo.Verify(x => x.CreateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ProcessVNPayPayment_Successfully()
    {
        var order = new Order { Id = Guid.NewGuid(), Code = "ORD-001", Subtotal = 1000, ShippingFee = 50, Status = OrderStatus.PaymentPending };
        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        
        var vnpSection = new Mock<IConfigurationSection>();
        vnpSection.Setup(x => x["PaymentUrl"]).Returns("https://vnpay.vn");
        vnpSection.Setup(x => x["HashSecret"]).Returns("secret");
        vnpSection.Setup(x => x["TmnCode"]).Returns("tmn");
        vnpSection.Setup(x => x["ReturnUrl"]).Returns("http://localhost/return");
        
        _config.Setup(x => x.GetSection("VNPay")).Returns(vnpSection.Object);

        var request = new CreatePaymentReq { OrderId = order.Id, PaymentMethod = PaymentMethod.VNPay, ReturnUrl = "http://client/thanks" };
        var command = new CreatePaymentCommand(request);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Url.Should().StartWith("https://vnpay.vn");
    }

    [Fact]
    public async Task Handle_Should_ProcessVouchersAndPoints_Successfully()
    {
        var customerId = Guid.NewGuid();
        var order = new Order { 
            Id = Guid.NewGuid(), 
            Code = "ORD-001", 
            Subtotal = 1000, 
            ShippingFee = 50, 
            Status = OrderStatus.PaymentPending,
            CustomerId = customerId
        };
        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
  
        var vouchers = new List<Voucher> {
            new Voucher { Code = "VOU-1", MinOrderValue = 100, Type = VoucherType.DISCOUNT_FIXED_AMOUNT, DiscountValue = 10 }
        };
        _engagementClient.Setup(x => x.GetMyVoucherAsync(customerId)).ReturnsAsync(vouchers.AsEnumerable());
        
        var userInfo = new UserInfo { 
            Email = "test@test.com", 
            LoyaltyTier = new LoyaltyRes { 
                CurrentTier = new MembershipTier { ValuePerPoint = 1000, DiscountRate = 0.1 } 
            } 
        };
        _identityClient.Setup(x => x.GetUserByIdAsync(customerId)).ReturnsAsync(userInfo);
        _engagementClient.Setup(x => x.GetMyPointByEmailAsync("test@test.com")).ReturnsAsync(100);

        var request = new CreatePaymentReq { 
            OrderId = order.Id, 
            PaymentMethod = PaymentMethod.Cash,
            VoucherDiscountCode = "VOU-1",
            Point = 10
        };
        var command = new CreatePaymentCommand(request);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.OrderSaleItems.Should().Contain(x => x.Type == OrderSaleItemType.Voucher);
        order.OrderSaleItems.Should().Contain(x => x.Type == OrderSaleItemType.LoyaltyPoint);
    }

    [Fact]
    public async Task Handle_Should_ProcessWallet_Successfully()
    {
        var customerId = Guid.NewGuid();
        var order = new Order { 
            Id = Guid.NewGuid(), 
            Code = "ORD-001", 
            Subtotal = 1000, 
            ShippingFee = 50, 
            Status = OrderStatus.PaymentPending,
            CustomerId = customerId
        };
        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = customerId, Balance = 500, RowVersion = new byte[] { 1 } };
        _walletRepo.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var request = new CreatePaymentReq { 
            OrderId = order.Id, 
            PaymentMethod = PaymentMethod.Cash,
            Wallet = 100
        };
        var command = new CreatePaymentCommand(request);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.OrderSaleItems.Should().Contain(x => x.Type == OrderSaleItemType.Wallet);
        _walletRepo.Verify(x => x.DeductBalanceAsync(wallet.Id, 100, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ProcessShippingVoucher_Successfully()
    {
        var customerId = Guid.NewGuid();
        var order = new Order { 
            Id = Guid.NewGuid(), 
            Code = "ORD-001", 
            Subtotal = 1000, 
            ShippingFee = 50, 
            Status = OrderStatus.PaymentPending,
            CustomerId = customerId
        };
        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var vouchers = new List<Voucher> {
            new Voucher { Code = "SHIP-1", MinOrderValue = 100, Type = VoucherType.DISCOUNT_FIXED_AMOUNT, DiscountValue = 30 }
        };
        _engagementClient.Setup(x => x.GetMyVoucherAsync(customerId)).ReturnsAsync(vouchers.AsEnumerable());
        _identityClient.Setup(x => x.GetUserByIdAsync(customerId)).ReturnsAsync(new UserInfo());

        var request = new CreatePaymentReq { 
            OrderId = order.Id, 
            PaymentMethod = PaymentMethod.Cash,
            VoucherShipCode = "SHIP-1"
        };
        var command = new CreatePaymentCommand(request);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.OrderSaleItems.Should().Contain(x => x.Name.Contains("Miễn phí Ship"));
    }

    [Fact]
    public async Task Handle_Should_ProcessVoucherGiftItems_Successfully()
    {
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new Order { 
            Id = orderId, 
            Code = "ORD-001", 
            Subtotal = 1000, 
            ShippingFee = 50, 
            Status = OrderStatus.PaymentPending,
            CustomerId = customerId,
            StoreId = Guid.NewGuid()
        };
        order.OrderItems.Add(new OrderItem { Sku = "SKU-REQ", Quantity = 1 });
        
        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
 
        var voucherId = Guid.NewGuid();
        var vouchers = new List<Voucher> {
            new Voucher { 
                Id = voucherId, 
                Code = "GIFT-1", 
                MinOrderValue = 100, 
                Type = VoucherType.GIFT, 
                VoucherItems = new List<VoucherItem> {
                    new VoucherItem { Sku = "SKU-REQ", Quantity = 1, Type = VoucherItemType.ITEM_REQUIRED, VoucherId = voucherId },
                    new VoucherItem { Sku = "SKU-GIFT", Quantity = 1, Type = VoucherItemType.ITEM_GIFT, VoucherId = voucherId }
                }
            }
        };
        _engagementClient.Setup(x => x.GetMyVoucherAsync(customerId)).ReturnsAsync(vouchers.AsEnumerable());
        
        _identityClient.Setup(x => x.GetUserByIdAsync(customerId)).ReturnsAsync(new UserInfo());

        var variants = new List<ProductVariantInfo> {
            new ProductVariantInfo { Sku = "SKU-GIFT", VariantId = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Gift Item", BasePrice = 0 }
        };
        _productClient.Setup(x => x.GetListProductVariantsBySkusAsync(It.IsAny<List<string>>())).ReturnsAsync(variants);
        _inventoryClient.Setup(x => x.ReleaseStockAsync(It.IsAny<ReleaseStockReq>())).ReturnsAsync(new ReleaseStockRes { Success = true });
        _inventoryClient.Setup(x => x.ReserveStockAsync(It.IsAny<ReserveStockReq>())).ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });

        var request = new CreatePaymentReq { 
            OrderId = order.Id, 
            PaymentMethod = PaymentMethod.Cash,
            VoucherDiscountCode = "GIFT-1"
        };
        var command = new CreatePaymentCommand(request);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.OrderItems.Any(x => x.Sku == "SKU-GIFT").Should().BeTrue();
    }
}
