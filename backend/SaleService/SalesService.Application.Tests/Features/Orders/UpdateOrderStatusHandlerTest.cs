using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.Dtos.Inventory;
using SalesService.Application.Features.Orders.Commands.UpdateOrderStatus;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace SalesService.Application.Tests.Features.Orders;

public class UpdateOrderStatusHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo;
    private readonly Mock<IPaymentRepository> _paymentRepo;
    private readonly Mock<IOutboxRepository> _outboxRepo;
    private readonly Mock<ILogger<UpdateOrderStatusCommandHandler>> _logger;
    private readonly Mock<IKafkaProducer> _kafkaProducer;
    private readonly Mock<IInventoryServiceClient> _inventoryClient;
    private readonly Mock<IWalletRepository> _walletRepo;
    private readonly Mock<IWalletTransactionRepository> _transactionRepo;
    private readonly UpdateOrderStatusCommandHandler _handler;

    public UpdateOrderStatusHandlerTests()
    {
        _orderRepo = new Mock<IOrderRepository>();
        _paymentRepo = new Mock<IPaymentRepository>();
        _outboxRepo = new Mock<IOutboxRepository>();
        _logger = new Mock<ILogger<UpdateOrderStatusCommandHandler>>();
        _kafkaProducer = new Mock<IKafkaProducer>();
        _inventoryClient = new Mock<IInventoryServiceClient>();
        _walletRepo = new Mock<IWalletRepository>();
        _transactionRepo = new Mock<IWalletTransactionRepository>();

        _handler = new UpdateOrderStatusCommandHandler(
            _orderRepo.Object,
            _paymentRepo.Object,
            _outboxRepo.Object,
            _logger.Object,
            _kafkaProducer.Object,
            _inventoryClient.Object,
            _walletRepo.Object,
            _transactionRepo.Object
        );
    }

    [Fact]
    public async Task Handle_WhenOrderIdIsEmpty_ReturnsErrorMessage()
    {
        var request = new UpdateOrderStatusReq { OrderId = Guid.Empty, Status = OrderStatus.Preparing };
        var result = await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        result.Should().Be("OrderId is required.");
    }

    [Fact]
    public async Task Handle_WhenOrderIsNotFound_ReturnsErrorMessage()
    {
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.Preparing };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        result.Should().Contain($"Order with id '{orderId}' not found.");
    }

    [Fact]
    public async Task Handle_WhenOrderIsDeleted_ReturnsErrorMessage()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, IsDeleted = true };
        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.Preparing };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        result.Should().Contain($"Order with id '{orderId}' not found.");
    }

    [Fact]
    public async Task Handle_WhenCustomerIdIsNull_ReturnsErrorMessage()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, CustomerId = null, IsDeleted = false };
        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.Preparing };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        result.Should().Be("CustomerId is null");
    }

    [Fact]
    public async Task Handle_WhenStatusTransitionIsInvalid_ReturnsErrorMessage()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, CustomerId = Guid.NewGuid(), Status = OrderStatus.Completed };

        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.PaymentPending };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        result.Should().Contain("Invalid order status transition");
    }

    [Fact]
    public async Task Handle_WhenCancelledAndNotConfirmationPending_ReleasesStockAndUpdatesStatus()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            Code = "ORD-ESPRESSO",
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.PaymentPending,
            StoreId = Guid.NewGuid(),
            OrderItems = []
        };

        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.Cancelled };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        result.Should().Contain("updated successfully to 'Cancelled'");
        _inventoryClient.Verify(x => x.ReleaseStockAsync(It.Is<ReleaseStockReq>(r => r.OrderId == orderId)), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancelledAndConfirmationPendingButWalletNotFound_ReturnsErrorMessage()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            Status = OrderStatus.ConfirmationPending,
            StoreId = Guid.NewGuid()
        };
        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.Cancelled };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _walletRepo.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        result.Should().Be("Cancel fail due to no wallet found");
    }

    [Fact]
    public async Task Handle_WhenCancelledAndConfirmationPendingButPaidAmountIsNull_ReturnsErrorMessage()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            Status = OrderStatus.ConfirmationPending,
            StoreId = Guid.NewGuid(),
            PaidAmount = null,
            OrderSaleItems = []
        };
        var wallet = new Wallet { Id = Guid.NewGuid(), RowVersion = [1] };
        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.Cancelled };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _walletRepo.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        result.Should().Contain("Cancel fail due to no PaidAmout found");
    }

    [Fact]
    public async Task Handle_WhenCancelledSuccessfullyAsStaff_RefundsWalletPointsVouchersAndPublishesEvents()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var rowVersion = new byte[] { 1, 2, 3 };

        var order = new Order
        {
            Id = orderId,
            Code = "ORD-COLD-BREW",
            CustomerId = customerId,
            Status = OrderStatus.ConfirmationPending,
            StoreId = Guid.NewGuid(),
            PaidAmount = 50000m,
            PaymentMethod = PaymentMethod.VNPay,
            OrderItems = [],
            OrderSaleItems =
            [
                new() { Type = OrderSaleItemType.Wallet, Amount = 10000m },
                new() { Type = OrderSaleItemType.LoyaltyPoint, Amount = 1500m }
            ]
        };

        var wallet = new Wallet { Id = walletId, Balance = 0m, RowVersion = rowVersion };
        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.Cancelled, RoleSlug = "staff" };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _walletRepo.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        result.Should().Contain("updated successfully to 'Cancelled'");

        // 10k Wallet + 50k VNPay = 60k total refund
        _transactionRepo.Verify(x => x.AddAsync(It.Is<WalletTransaction>(w => w.Amount == 60000m && w.TransactionType == TransactionType.Withdrawal), It.IsAny<CancellationToken>()), Times.Once);
        _walletRepo.Verify(x => x.AddBalanceAsync(walletId, 60000m, rowVersion, It.IsAny<CancellationToken>()), Times.Once);

        _kafkaProducer.Verify(x => x.PublishAsync(KafkaTopics.UpdateUserPoint, It.Is<UpdatePointEvent>(e => e.Point == 1500m && e.ActionType == UpdatePointType.Add)), Times.Once);
        _kafkaProducer.Verify(x => x.PublishAsync(KafkaTopics.ReverseUserVoucher, It.Is<ReverseVoucherEvent>(e => e.OrderId == orderId)), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCompletedWithVNPayButPaymentNotFound_ReturnsErrorMessage()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Delivering,
            PaymentMethod = PaymentMethod.VNPay
        };
        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.Completed };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _paymentRepo.Setup(x => x.GetNewestByOrderIdAndStatusAsync(orderId, PaymentStatus.Paid, It.IsAny<CancellationToken>())).ReturnsAsync((Payment?)null);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        result.Should().Be("Order cannot be completed because payment is not paid.");
    }

    [Fact]
    public async Task Handle_WhenCompletedWithCashButNoPendingPayment_ReturnsErrorMessage()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Delivering,
            PaymentMethod = PaymentMethod.Cash
        };
        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.Completed };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _paymentRepo.Setup(x => x.GetNewestByOrderIdAndStatusAsync(orderId, PaymentStatus.Pending, It.IsAny<CancellationToken>())).ReturnsAsync((Payment?)null);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        result.Should().Be("No pending payment found for this order.");
    }

    [Fact]
    public async Task Handle_WhenCompletedSuccessfullyWithCash_UpdatesPaymentAndPublishesEvents()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            Code = "ORD-MATCHA-LATTE",
            CustomerId = customerId,
            Status = OrderStatus.Delivering,
            PaymentMethod = PaymentMethod.Cash,
            PaidAmount = 65000m,
            OrderItems = []
        };
        var payment = new Payment { Id = Guid.NewGuid(), OrderId = orderId, Status = (int)PaymentStatus.Pending, Order = order };
        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.Completed };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _paymentRepo.Setup(x => x.GetNewestByOrderIdAndStatusAsync(orderId, PaymentStatus.Pending, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        result.Should().Contain("updated successfully to 'Completed'");

        // Verify Payment updated to Paid
        payment.Status.Should().Be((int)PaymentStatus.Paid);
        _paymentRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify Reward Event published
        _kafkaProducer.Verify(x => x.PublishAsync(KafkaTopics.ProcessOrderReward, It.Is<ProcessOrderRewardEvent>(e => e.Id == customerId && e.OrderAmount == 65000m)), Times.Once);

        // ✅ ĐÃ FIX: Verify hàm Add của Interface (chứa cái Message đã được Serialize bởi Extension Method)
        _outboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m =>
            m.Type == KafkaTopics.OrderPaid &&
            m.Content.Contains("OrderPaidEvent") // Có thể check thêm một đoạn content json nếu cần
        )), Times.Once);

        _outboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m =>
            m.Type == "OrderStatusEvent"
        )), Times.Once);
    }
}