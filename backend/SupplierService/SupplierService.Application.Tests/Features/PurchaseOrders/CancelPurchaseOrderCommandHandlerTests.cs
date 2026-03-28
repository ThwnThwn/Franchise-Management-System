using FluentAssertions;
using Moq;
using SupplierService.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Tests.Features.PurchaseOrders;

public class CancelPurchaseOrderCommandHandlerTests
{
    private readonly Mock<IPurchaseOrderRepository> _purchaseOrderRepoMock;
    private readonly CancelPurchaseOrderCommandHandler _handler;

    public CancelPurchaseOrderCommandHandlerTests()
    {
        _purchaseOrderRepoMock = new Mock<IPurchaseOrderRepository>();
        _handler = new CancelPurchaseOrderCommandHandler(_purchaseOrderRepoMock.Object);
    }

    #region 1. Validation & Exception Tests

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenPurchaseOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new CancelPurchaseOrderCommand(orderId, "Nhập sai số lượng hạt Arabica");

        _purchaseOrderRepoMock
            .Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrder?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Purchase order {orderId} not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStatusIsCompleted()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new CancelPurchaseOrderCommand(orderId, "Hủy đơn");
        var po = new PurchaseOrder { Id = orderId, Status = PurchaseOrderStatus.Completed };

        _purchaseOrderRepoMock
            .Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot cancel a completed purchase order.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStatusIsAlreadyCancelled()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new CancelPurchaseOrderCommand(orderId, "Hủy đơn lần nữa");
        var po = new PurchaseOrder { Id = orderId, Status = PurchaseOrderStatus.Cancelled };

        _purchaseOrderRepoMock
            .Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Purchase order is already cancelled.");
    }

    #endregion 1. Validation & Exception Tests

    #region 2. Success & Logic Branch Tests

    [Theory]
    [InlineData(PurchaseOrderStatus.Pending)]
    [InlineData(PurchaseOrderStatus.ConfirmPending)]
    [InlineData(PurchaseOrderStatus.Confirmed)]
    public async Task Handle_ShouldUpdateStatusToCancelled_ForValidInitialStatuses(PurchaseOrderStatus initialStatus)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new CancelPurchaseOrderCommand(orderId, "Hủy đơn hàng cà phê cho chi nhánh Q1");
        var po = new PurchaseOrder { Id = orderId, Status = initialStatus };

        _purchaseOrderRepoMock
            .Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        po.Status.Should().Be(PurchaseOrderStatus.Cancelled);
        _purchaseOrderRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetNewNotes_WhenOriginalNotesAreEmpty()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var reason = "Nhà cung cấp báo hết Robusta";
        var command = new CancelPurchaseOrderCommand(orderId, reason);
        var po = new PurchaseOrder
        {
            Id = orderId,
            Status = PurchaseOrderStatus.Pending,
            Notes = null
        };

        _purchaseOrderRepoMock
            .Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        po.Notes.Should().Be($"[Cancelled] {reason}");
    }

    [Fact]
    public async Task Handle_ShouldAppendToNotes_WhenOriginalNotesExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var originalNotes = "Đơn hàng nhập 50kg cà phê rang xay";
        var reason = "Đổi sang lấy cà phê hạt nguyên chất";
        var command = new CancelPurchaseOrderCommand(orderId, reason);
        var po = new PurchaseOrder
        {
            Id = orderId,
            Status = PurchaseOrderStatus.Confirmed,
            Notes = originalNotes
        };

        _purchaseOrderRepoMock
            .Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        po.Notes.Should().Be($"{originalNotes}\n[Cancelled] {reason}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldNotUpdateNotes_WhenReasonIsMissing(string? emptyReason)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var originalNotes = "Ghi chú ban đầu";
        var command = new CancelPurchaseOrderCommand(orderId, emptyReason);
        var po = new PurchaseOrder
        {
            Id = orderId,
            Status = PurchaseOrderStatus.Pending,
            Notes = originalNotes
        };

        _purchaseOrderRepoMock
            .Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        po.Status.Should().Be(PurchaseOrderStatus.Cancelled);
        po.Notes.Should().Be(originalNotes, "Reason trống thì giữ nguyên Notes cũ");
    }

    #endregion 2. Success & Logic Branch Tests
}