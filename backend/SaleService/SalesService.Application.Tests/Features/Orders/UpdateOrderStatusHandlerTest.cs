using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.Features.Orders.Commands.UpdateOrderStatus;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Infrastructure.Kafka;

namespace SalesService.Application.Tests.Features.Orders;

public class UpdateOrderStatusCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
    private readonly Mock<ILogger<UpdateOrderStatusCommandHandler>> _loggerMock;
    private readonly Mock<IKafkaProducer> _kafkaProducerMock;
    private readonly Mock<IInventoryServiceClient> _inventoryServiceClientMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly UpdateOrderStatusCommandHandler _handler;

    public UpdateOrderStatusCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _outboxRepositoryMock = new Mock<IOutboxRepository>();
        _loggerMock = new Mock<ILogger<UpdateOrderStatusCommandHandler>>();
        _kafkaProducerMock = new Mock<IKafkaProducer>();
        _inventoryServiceClientMock = new Mock<IInventoryServiceClient>();
        _walletRepositoryMock = new Mock<IWalletRepository>();

        _handler = new UpdateOrderStatusCommandHandler(
            _orderRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _outboxRepositoryMock.Object,
            _loggerMock.Object,
            _kafkaProducerMock.Object,
            _inventoryServiceClientMock.Object,
            _walletRepositoryMock.Object);
    }

    /// <summary>
    /// Tạo order hợp lệ với trạng thái chỉ định
    /// </summary>
    private Order CreateOrder(OrderStatus status, PaymentMethod? paymentMethod = null, Guid? customerId = null)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            Code = "ORD-TEST-001",
            CustomerId = customerId ?? Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            Status = status,
            PaymentMethod = paymentMethod,
            TotalAmount = 100_000m,
            Tax = 10_000m,
            OrderItems = new List<OrderItem>()
        };
    }

    // ============================
    // NHÓM 1: VALIDATE ĐẦU VÀO
    // ============================

    // 1. LỖI - OrderId rỗng → trả về thông báo lỗi
    [Fact]
    public async Task Handle_ShouldReturnError_WhenOrderIdIsEmpty()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = Guid.Empty,
            Status = OrderStatus.ConfirmationPending
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("OrderId is required.");

        _orderRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // 2. LỖI - Không tìm thấy order
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.ConfirmationPending
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Order with id '{orderId}' not found.");
    }

    // 3. LỖI - Order đã bị xoá (IsDeleted = true)
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderIsDeleted()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateOrder(OrderStatus.PaymentPending);
        order.Id = orderId;
        order.IsDeleted = true; // Đã xoá

        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.ConfirmationPending
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Order with id '{orderId}' not found.");
    }

    // 4. LỖI - Order không có CustomerId
    [Fact]
    public async Task Handle_ShouldReturnError_WhenOrderHasNoCustomerId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateOrder(OrderStatus.PaymentPending, customerId: null);
        order.Id = orderId;
        order.CustomerId = null; // Không có customer

        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.ConfirmationPending
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("CustomerId is null");
    }

    // 5. LỖI - Chuyển trạng thái không hợp lệ (vi phạm TransitionPolicy)
    [Fact]
    public async Task Handle_ShouldReturnError_WhenStatusTransitionIsInvalid()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateOrder(OrderStatus.Completed); // Đã hoàn thành, không thể chuyển sang trạng thái nào
        order.Id = orderId;

        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.Cancelled // Không hợp lệ: Completed → Cancelled
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Invalid order status transition from {OrderStatus.Completed} to {OrderStatus.Cancelled}");
    }

    // ============================
    // NHÓM 2: THÀNH CÔNG - UPDATE STATUS
    // ============================

    // 6. THÀNH CÔNG - PaymentPending → ConfirmationPending (thanh toán xong)
    [Fact]
    public async Task Handle_ShouldUpdateStatus_FromPaymentPendingToConfirmationPending()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateOrder(OrderStatus.PaymentPending, PaymentMethod.Cash);
        order.Id = orderId;

        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.ConfirmationPending
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _outboxRepositoryMock.Setup(x => x.Add(It.IsAny<OutboxMessage>()));

        _kafkaProducerMock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Order status updated successfully to '{OrderStatus.ConfirmationPending}'");
        order.Status.Should().Be(OrderStatus.ConfirmationPending);

        _orderRepositoryMock.Verify(
            x => x.UpdateAsync(order, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // 7. THÀNH CÔNG - ConfirmationPending → Confirmed
    [Fact]
    public async Task Handle_ShouldUpdateStatus_FromConfirmationPendingToConfirmed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateOrder(OrderStatus.ConfirmationPending, PaymentMethod.Cash);
        order.Id = orderId;

        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.Confirmed
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _outboxRepositoryMock.Setup(x => x.Add(It.IsAny<OutboxMessage>()));
        _kafkaProducerMock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Order status updated successfully to '{OrderStatus.Confirmed}'");
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    // ============================
    // NHÓM 3: HỦY ĐƠN HÀNG
    // ============================

    // 8. THÀNH CÔNG - Hủy đơn PaymentPending → giải phóng tồn kho
    [Fact]
    public async Task Handle_ShouldCancelOrder_WhenStatusIsPaymentPending()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateOrder(OrderStatus.PaymentPending, PaymentMethod.Cash);
        order.Id = orderId;
        order.StoreId = Guid.NewGuid();

        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.Cancelled
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _inventoryServiceClientMock
            .Setup(x => x.ReleaseStockAsync(It.IsAny<ReleaseStockReq>()))
            .ReturnsAsync(new ReleaseStockRes { Success = true });

        _outboxRepositoryMock.Setup(x => x.Add(It.IsAny<OutboxMessage>()));
        _kafkaProducerMock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Order status updated successfully to '{OrderStatus.Cancelled}'");
        order.Status.Should().Be(OrderStatus.Cancelled);

        // Xác nhận đã giải phóng tồn kho
        _inventoryServiceClientMock.Verify(
            x => x.ReleaseStockAsync(It.Is<ReleaseStockReq>(r => r.OrderId == orderId)),
            Times.Once);
    }

    // 9. LỖI - Hủy đơn ConfirmationPending + VNPay nhưng không tìm thấy ví
    [Fact]
    public async Task Handle_ShouldReturnError_WhenCancelVnPayOrderAndWalletNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = CreateOrder(OrderStatus.ConfirmationPending, PaymentMethod.VNPay, customerId);
        order.Id = orderId;
        order.PaidAmount = 100_000m;

        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.Cancelled
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _inventoryServiceClientMock
            .Setup(x => x.ReleaseStockAsync(It.IsAny<ReleaseStockReq>()))
            .ReturnsAsync(new ReleaseStockRes { Success = true });

        // Không tìm thấy ví
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("Cancel fail due to no wallet found");
    }

    // ============================
    // NHÓM 4: HOÀN THÀNH ĐƠN HÀNG
    // ============================

    // 10. THÀNH CÔNG - Hoàn thành đơn COD (Cash) từ ReadyForPickup → Completed
    [Fact]
    public async Task Handle_ShouldCompleteOrder_WhenPaymentMethodIsCashAndPendingPaymentExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateOrder(OrderStatus.ReadyForPickup, PaymentMethod.Cash);
        order.Id = orderId;

        var pendingPayment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Order = order,
            Status = (int)PaymentStatus.Pending
        };

        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.Completed
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentRepositoryMock
            .Setup(x => x.GetNewestByOrderIdAndStatusAsync(orderId, PaymentStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingPayment);

        _paymentRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _outboxRepositoryMock.Setup(x => x.Add(It.IsAny<OutboxMessage>()));

        _kafkaProducerMock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Order status updated successfully to '{OrderStatus.Completed}'");
        order.Status.Should().Be(OrderStatus.Completed);
        pendingPayment.Status.Should().Be((int)PaymentStatus.Paid);

        // Xác nhận SaveChanges được gọi để cập nhật payment
        _paymentRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // 11. LỖI - Hoàn thành đơn COD nhưng không tìm thấy Pending Payment
    [Fact]
    public async Task Handle_ShouldReturnError_WhenCompletingCashOrderAndNoPendingPaymentFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateOrder(OrderStatus.ReadyForPickup, PaymentMethod.Cash);
        order.Id = orderId;

        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.Completed
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Không có pending payment
        _paymentRepositoryMock
            .Setup(x => x.GetNewestByOrderIdAndStatusAsync(orderId, PaymentStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("No pending payment found for this order.");
    }

    // 12. LỖI - Hoàn thành đơn VNPay nhưng chưa thanh toán (không có Paid payment)
    [Fact]
    public async Task Handle_ShouldReturnError_WhenCompletingVnPayOrderAndPaymentNotPaid()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateOrder(OrderStatus.ReadyForPickup, PaymentMethod.VNPay);
        order.Id = orderId;

        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.Completed
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Không có Paid payment
        _paymentRepositoryMock
            .Setup(x => x.GetNewestByOrderIdAndStatusAsync(orderId, PaymentStatus.Paid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("Order cannot be completed because payment is not paid.");
    }

    // 13. THÀNH CÔNG - Hủy đơn ConfirmationPending + VNPay (Staff hủy) → Hoàn tiền + Hoàn Point/Voucher
    [Fact]
    public async Task Handle_ShouldRefundWalletAndReversePoints_WhenStaffCancelsPaidVnPayOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = CreateOrder(OrderStatus.ConfirmationPending, PaymentMethod.VNPay, customerId);
        order.Id = orderId;
        order.PaidAmount = 100_000m;
        order.OrderSaleItems = new List<OrderSaleItem>
        {
            new OrderSaleItem { Type = OrderSaleItemType.LoyaltyPoint, Amount = 50 }
        };

        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.Cancelled,
            RoleSlug = "staff"
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _inventoryServiceClientMock
            .Setup(x => x.ReleaseStockAsync(It.IsAny<ReleaseStockReq>()))
            .ReturnsAsync(new ReleaseStockRes { Success = true });

        var wallet = new Wallet { Id = Guid.NewGuid(), RowVersion = new byte[] { 1 } };
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        _walletRepositoryMock.Setup(x => x.AddBalanceAsync(wallet.Id, 100_000m, wallet.RowVersion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Contain("successfully");
        _walletRepositoryMock.Verify(x => x.AddBalanceAsync(wallet.Id, 100_000m, wallet.RowVersion, It.IsAny<CancellationToken>()), Times.Once);
        _kafkaProducerMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeast(2)); // Reversal events + OrderStatusEvent
    }

    // 14. LỖI - Hủy đơn ConfirmationPending + VNPay nhưng PaidAmount null
    [Fact]
    public async Task Handle_ShouldReturnError_WhenCancelPaidVnPayOrderButPaidAmountIsNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = CreateOrder(OrderStatus.ConfirmationPending, PaymentMethod.VNPay, customerId);
        order.Id = orderId;
        order.PaidAmount = null; // NULL

        var command = new UpdateOrderStatusCommand(new UpdateOrderStatusReq
        {
            OrderId = orderId,
            Status = OrderStatus.Cancelled
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _inventoryServiceClientMock
            .Setup(x => x.ReleaseStockAsync(It.IsAny<ReleaseStockReq>()))
            .ReturnsAsync(new ReleaseStockRes { Success = true });

        var wallet = new Wallet { Id = Guid.NewGuid(), RowVersion = new byte[] { 1 } };
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Cancel fail due to no PaidAmout found in order with ID {orderId}");
    }
}
