using FluentAssertions;
using Moq;
using SupplierService.Application.ClientInterfaces;
using SupplierService.Application.Features.PurchaseOrders.Commands.CompletePurchaseOrder;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Tests.Features.PurchaseOrders;

public class CompletePurchaseOrderHandlerTests
{
    private readonly Mock<IPurchaseOrderRepository> _purchaseOrderRepoMock;
    private readonly Mock<IInventoryServiceClient> _inventoryClientMock;
    private readonly CompletePurchaseOrderCommandHandler _handler;

    public CompletePurchaseOrderHandlerTests()
    {
        _purchaseOrderRepoMock = new Mock<IPurchaseOrderRepository>();
        _inventoryClientMock = new Mock<IInventoryServiceClient>();

        _handler = new CompletePurchaseOrderCommandHandler(
            _purchaseOrderRepoMock.Object,
            _inventoryClientMock.Object);
    }

    #region 1. Validation & Exception Tests

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenOrderNotFound()
    {
        // Arrange
        var command = new CompletePurchaseOrderCommand(Guid.NewGuid());

        _purchaseOrderRepoMock
            .Setup(repo => repo.GetByIdWithItemsAsync(command.PurchaseOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrder?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Purchase order {command.PurchaseOrderId} not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenOrderIsAlreadyCompleted()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new CompletePurchaseOrderCommand(orderId);
        var po = new PurchaseOrder
        {
            Id = orderId,
            Status = PurchaseOrderStatus.Completed,
            Items = new List<PurchaseOrderItem>() // Khởi tạo để né NullReferenceException
        };

        _purchaseOrderRepoMock
            .Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Purchase order is already completed.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenOrderIsCancelled()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new CompletePurchaseOrderCommand(orderId);
        var po = new PurchaseOrder
        {
            Id = orderId,
            Status = PurchaseOrderStatus.Cancelled,
            Items = new List<PurchaseOrderItem>()
        };

        _purchaseOrderRepoMock
            .Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot complete a cancelled purchase order.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStoreIdIsNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new CompletePurchaseOrderCommand(orderId);
        var po = new PurchaseOrder
        {
            Id = orderId,
            Status = PurchaseOrderStatus.Confirmed,
            StoreId = null, // Lỗi ở đây: Đơn nhập nhưng không biết nhập vào kho nào
            Items = new List<PurchaseOrderItem>()
        };

        _purchaseOrderRepoMock
            .Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Purchase order has no target store. Please set StoreId before completing.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenOrderHasNoItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new CompletePurchaseOrderCommand(orderId);
        var po = new PurchaseOrder
        {
            Id = orderId,
            Status = PurchaseOrderStatus.Confirmed,
            StoreId = Guid.NewGuid(), // Kho chi nhánh Q1
            Items = new List<PurchaseOrderItem>() // Lỗi ở đây: Đơn rỗng, không có món nào
        };

        _purchaseOrderRepoMock
            .Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Purchase order has no items. Cannot complete an empty order.");
    }

    #endregion 1. Validation & Exception Tests

    #region 2. Success Path & gRPC Integration Test

    [Fact]
    public async Task Handle_ShouldCompleteOrder_AndCallGrpcToIncreaseStock_WhenValid()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var storeId = Guid.NewGuid(); // Kho chi nhánh Quận 1
        var command = new CompletePurchaseOrderCommand(orderId);

        // Setup đơn nhập hàng đậm chất F&B
        var po = new PurchaseOrder
        {
            Id = orderId,
            Status = PurchaseOrderStatus.Confirmed,
            PaymentStatus = PaymentStatus.Pending,
            StoreId = storeId,
            Items = new List<PurchaseOrderItem>
            {
                // Nhập 50kg hạt Arabica nguyên chất
                new PurchaseOrderItem { IngredientId = Guid.NewGuid(), Quantity = 50, UnitPrice = 150000 },
                // Nhập 100 hộp Sữa đặc Ngôi Sao Phương Nam
                new PurchaseOrderItem { IngredientId = Guid.NewGuid(), Quantity = 100, UnitPrice = 25000 }
            }
        };

        _purchaseOrderRepoMock
            .Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - 1. Kiểm tra cập nhật Entity
        po.Status.Should().Be(PurchaseOrderStatus.Completed);
        po.PaymentStatus.Should().Be(PaymentStatus.Paid);
        po.CompletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2)); // Cấp biên độ trễ 2 giây cho an toàn

        // Assert - 2. Đảm bảo đã gọi lưu DB
        _purchaseOrderRepoMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Assert - 3. Đảm bảo đã gọi gRPC sang InventoryService với đúng StoreId và đúng số lượng Item
        _inventoryClientMock.Verify(client =>
            client.IncreaseStockAsync(
                storeId,
                It.Is<IEnumerable<IngredientStockEntry>>(entries => entries.Count() == 2),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "Phải gọi gRPC để cộng 2 loại nguyên liệu vào kho chi nhánh.");
    }

    #endregion 2. Success Path & gRPC Integration Test
}