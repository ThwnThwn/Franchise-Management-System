using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using SalesService.Application.AppExceptions;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.Features.Orders.Commands.UpdateOrderStatus;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Infrastructure.Kafka;

namespace SalesService.Application.Tests.Features.Orders;

public class UpdateOrderStatusHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
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
        _uowMock = new Mock<IUnitOfWork>();
        _orderRepo = new Mock<IOrderRepository>();
        _paymentRepo = new Mock<IPaymentRepository>();
        _outboxRepo = new Mock<IOutboxRepository>();
        _logger = new Mock<ILogger<UpdateOrderStatusCommandHandler>>();
        _kafkaProducer = new Mock<IKafkaProducer>();
        _inventoryClient = new Mock<IInventoryServiceClient>();
        _walletRepo = new Mock<IWalletRepository>();
        _transactionRepo = new Mock<IWalletTransactionRepository>();

        var dbTxMock = new Mock<IDbContextTransaction>();
        _uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbTxMock.Object);
        _uowMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _handler = new UpdateOrderStatusCommandHandler(
            _uowMock.Object,
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
    public async Task Handle_WhenExceptionOccurs_RollbacksTransactionAndRethrows()
    {
        var orderId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.PaymentPending,
            StoreId = Guid.NewGuid(),
        };

        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.Cancelled };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        // Mô phỏng UoW Commit nổ Exception
        _uowMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database timeout connection"));

        var action = async () => await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        await action.Should().ThrowAsync<Exception>().WithMessage("Database timeout connection");

        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderIdIsEmpty_ThrowsKeyNotFoundException()
    {
        var request = new UpdateOrderStatusReq { OrderId = Guid.Empty, Status = OrderStatus.Preparing };
        var action = async () => await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>().WithMessage("OrderId is required.");
    }

    [Fact]
    public async Task Handle_WhenOrderIsNotFound_ThrowsKeyNotFoundException()
    {
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.Preparing };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var action = async () => await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{orderId}*");
    }

    [Fact]
    public async Task Handle_WhenStatusTransitionIsInvalid_ThrowsBusinessException()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, CustomerId = Guid.NewGuid(), Status = OrderStatus.Completed };
        var request = new UpdateOrderStatusReq { OrderId = orderId, Status = OrderStatus.PaymentPending };

        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var action = async () => await _handler.Handle(new UpdateOrderStatusCommand(request), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessException>().WithMessage("Invalid order status transition*");
    }

    [Fact]
    public async Task Handle_WhenCancelledSuccessfullyAsStaff_RefundsWalletPointsVouchersAndCommits()
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

        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

        _transactionRepo.Verify(x => x.Add(It.Is<WalletTransaction>(w => w.Amount == 60000m && w.TransactionType == TransactionType.Withdrawal)), Times.Once);

        _walletRepo.Verify(x => x.ApplyAddBalance(wallet, 60000m, rowVersion), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCompletedSuccessfullyWithCash_UpdatesPaymentCommitsAndPublishesEvents()
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

        payment.Status.Should().Be((int)PaymentStatus.Paid);

        // Xác minh chỉ gọi Commit trên UoW, KHÔNG CÒN SaveChanges ngầm
        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

        _outboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m => m.Type == "OrderStatusEvent")), Times.Once);
    }
}