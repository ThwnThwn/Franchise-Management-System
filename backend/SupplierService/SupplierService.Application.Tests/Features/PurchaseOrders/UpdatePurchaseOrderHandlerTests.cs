using FluentAssertions;
using Moq;
using SupplierService.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using SupplierService.Application.Features.PurchaseOrders.Commands.UpdatePurchaseOrder;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Tests.Features.PurchaseOrders;

public class UpdatePurchaseOrderHandlerTests
{
    private readonly Mock<IPurchaseOrderRepository> _purchaseOrderRepoMock;
    private readonly UpdatePurchaseOrderCommandHandler _handler;

    public UpdatePurchaseOrderHandlerTests()
    {
        _purchaseOrderRepoMock = new Mock<IPurchaseOrderRepository>();
        _handler = new UpdatePurchaseOrderCommandHandler(_purchaseOrderRepoMock.Object);
    }

    #region 1. Validation Logic (Mọi nhánh ném Exception)

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenOrderNotFound()
    {
        // Arrange
        var command = new UpdatePurchaseOrderCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Đổi món", null, null, 0, []);

        _purchaseOrderRepoMock
            .Setup(repo => repo.GetByIdWithItemsAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrder?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Purchase order {command.Id} not found.");
    }

    [Theory]
    [InlineData(PurchaseOrderStatus.ConfirmPending)]
    [InlineData(PurchaseOrderStatus.Confirmed)]
    [InlineData(PurchaseOrderStatus.Completed)]
    [InlineData(PurchaseOrderStatus.Cancelled)]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStatusIsNotPending(PurchaseOrderStatus invalidStatus)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new UpdatePurchaseOrderCommand(orderId, Guid.NewGuid(), null, null, null, null, 0, new List<CreatePurchaseOrderItemDto>());

        var po = new PurchaseOrder
        {
            Id = orderId,
            Status = invalidStatus,
            Items = [] // Rule 5: Khởi tạo an toàn
        };

        _purchaseOrderRepoMock.Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(po);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Cannot update purchase order with status '{invalidStatus}'. Only 'Pending' orders can be updated.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenSupplierDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new UpdatePurchaseOrderCommand(orderId, Guid.NewGuid(), null, null, null, null, 0, new List<CreatePurchaseOrderItemDto>());

        var po = new PurchaseOrder { Id = orderId, Status = PurchaseOrderStatus.Pending, Items = [] };

        _purchaseOrderRepoMock.Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(po);
        _purchaseOrderRepoMock.Setup(repo => repo.SupplierExistsAsync(command.SupplierId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Supplier {command.SupplierId} not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenItemsAreNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new UpdatePurchaseOrderCommand(orderId, Guid.NewGuid(), null, null, null, null, 0, null!);

        var po = new PurchaseOrder { Id = orderId, Status = PurchaseOrderStatus.Pending, Items = [] };

        _purchaseOrderRepoMock.Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(po);
        _purchaseOrderRepoMock.Setup(repo => repo.SupplierExistsAsync(command.SupplierId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Purchase order must have at least one item.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenItemsAreEmpty()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new UpdatePurchaseOrderCommand(orderId, Guid.NewGuid(), null, null, null, null, 0, new List<CreatePurchaseOrderItemDto>());

        var po = new PurchaseOrder { Id = orderId, Status = PurchaseOrderStatus.Pending, Items = [] };

        _purchaseOrderRepoMock.Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(po);
        _purchaseOrderRepoMock.Setup(repo => repo.SupplierExistsAsync(command.SupplierId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Purchase order must have at least one item.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenIngredientsAreMissingFromSupplierPriceList()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var ingredient1 = Guid.NewGuid(); // Hạt Robusta
        var ingredient2 = Guid.NewGuid(); // Sữa tươi Thanh Trùng (Chưa có giá)

        var newItems = new List<CreatePurchaseOrderItemDto>
        {
            new(ingredient1, 10),
            new(ingredient2, 5)
        };

        var command = new UpdatePurchaseOrderCommand(orderId, Guid.NewGuid(), null, null, null, null, 0, newItems);
        var po = new PurchaseOrder { Id = orderId, Status = PurchaseOrderStatus.Pending, Items = [] };

        _purchaseOrderRepoMock.Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(po);
        _purchaseOrderRepoMock.Setup(repo => repo.SupplierExistsAsync(command.SupplierId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Chỉ trả về giá của món 1
        var priceMap = new Dictionary<Guid, decimal> { { ingredient1, 120000m } };
        _purchaseOrderRepoMock
            .Setup(repo => repo.GetSupplierIngredientPricesAsync(command.SupplierId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceMap);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Ingredients not found for this supplier: {ingredient2}");
    }

    #endregion 1. Validation Logic (Mọi nhánh ném Exception)

    #region 2. Happy Path (Luồng Xóa cũ - Cập nhật mới - Tính tiền)

    [Fact]
    public async Task Handle_ShouldClearOldItems_AddNewItems_AndRecalculateAmounts_WhenValid()
    {
        // Arrange - Rule 1: Dữ liệu F&B
        var orderId = Guid.NewGuid();
        var newStoreId = Guid.NewGuid(); // Chi nhánh mới
        var newSupplierId = Guid.NewGuid(); // Đổi nhà cung cấp Cà phê

        var newIngredient1 = Guid.NewGuid(); // Bột Cacao
        var newIngredient2 = Guid.NewGuid(); // Đường Đen

        var discountAmount = 20000m; // Giảm giá 20k

        var command = new UpdatePurchaseOrderCommand(
            orderId,
            newSupplierId,
            newStoreId,
            "Cập nhật lại số lượng cuối tuần",
            "REF-002",
            DateTimeOffset.UtcNow.AddDays(1),
            discountAmount,
            new List<CreatePurchaseOrderItemDto>
            {
                new(newIngredient1, 10m), // 10kg Cacao
                new(newIngredient2, 20m)  // 20kg Đường Đen
            });

        // Đơn hàng cũ đang có 1 món rác, cần phải được Clear
        var po = new PurchaseOrder
        {
            Id = orderId,
            Status = PurchaseOrderStatus.Pending,
            SupplierId = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            TotalAmount = 500000m,
            NetAmount = 500000m,
            Items = [new PurchaseOrderItem { IngredientId = Guid.NewGuid(), Quantity = 5, UnitPrice = 100000m }]
        };

        _purchaseOrderRepoMock.Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(po);
        _purchaseOrderRepoMock.Setup(repo => repo.SupplierExistsAsync(newSupplierId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Bảng giá nhà cung cấp mới: Cacao 80k/kg, Đường Đen 30k/kg
        var priceMap = new Dictionary<Guid, decimal>
        {
            { newIngredient1, 80000m },
            { newIngredient2, 30000m }
        };
        _purchaseOrderRepoMock
            .Setup(repo => repo.GetSupplierIngredientPricesAsync(newSupplierId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceMap);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert 1: Kiểm tra cập nhật thông tin chung
        po.SupplierId.Should().Be(newSupplierId);
        po.StoreId.Should().Be(newStoreId);
        po.Notes.Should().Be(command.Notes);
        po.ReferenceCode.Should().Be(command.ReferenceCode);
        po.ExpectedDeliveryDate.Should().Be(command.ExpectedDeliveryDate);
        po.DiscountAmount.Should().Be(discountAmount);

        // Assert 2: Kiểm tra Items cũ đã bị xóa và thay bằng 2 món mới
        po.Items.Should().HaveCount(2);
        po.Items.Should().Contain(i => i.IngredientId == newIngredient1 && i.Quantity == 10m && i.UnitPrice == 80000m && i.TotalPrice == 800000m);
        po.Items.Should().Contain(i => i.IngredientId == newIngredient2 && i.Quantity == 20m && i.UnitPrice == 30000m && i.TotalPrice == 600000m);

        // Assert 3: Kiểm tra phép tính tổng tiền
        // TotalAmount = (10 * 80.000) + (20 * 30.000) = 800.000 + 600.000 = 1.400.000
        var expectedTotalAmount = 1400000m;
        po.TotalAmount.Should().Be(expectedTotalAmount);

        // NetAmount = Total - Discount = 1.400.000 - 20.000 = 1.380.000
        var expectedNetAmount = 1380000m;
        po.NetAmount.Should().Be(expectedNetAmount);

        // Assert 4: Đảm bảo gọi hàm Save
        _purchaseOrderRepoMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion 2. Happy Path (Luồng Xóa cũ - Cập nhật mới - Tính tiền)
}