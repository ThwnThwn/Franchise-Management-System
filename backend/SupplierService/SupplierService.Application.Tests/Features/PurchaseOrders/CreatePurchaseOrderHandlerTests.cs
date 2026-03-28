using FluentAssertions;
using Moq;
using SupplierService.Application.ClientInterfaces;
using SupplierService.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Tests.Features.PurchaseOrders;

public class CreatePurchaseOrderHandlerTests
{
    private readonly Mock<IPurchaseOrderRepository> _purchaseOrderRepoMock;
    private readonly Mock<IFranchiseServiceClient> _franchiseClientMock;
    private readonly CreatePurchaseOrderCommandHandler _handler;

    public CreatePurchaseOrderHandlerTests()
    {
        _purchaseOrderRepoMock = new Mock<IPurchaseOrderRepository>();
        _franchiseClientMock = new Mock<IFranchiseServiceClient>();

        _handler = new CreatePurchaseOrderCommandHandler(
            _purchaseOrderRepoMock.Object,
            _franchiseClientMock.Object);
    }

    #region 1. Validation Tests (Các nhánh văng Exception)

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenSupplierDoesNotExist()
    {
        // Arrange
        var command = new CreatePurchaseOrderCommand(Guid.NewGuid(), Guid.NewGuid(), "Nhập gấp", null, null, 0, new List<CreatePurchaseOrderItemDto>());

        _purchaseOrderRepoMock
            .Setup(x => x.SupplierExistsAsync(command.SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Supplier {command.SupplierId} not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenItemsListIsNull()
    {
        // Arrange
        var command = new CreatePurchaseOrderCommand(Guid.NewGuid(), Guid.NewGuid(), null, null, null, 0, null!); // Cố tình truyền null

        _purchaseOrderRepoMock.Setup(x => x.SupplierExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Purchase order must have at least one item.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenItemsListIsEmpty()
    {
        // Arrange
        var command = new CreatePurchaseOrderCommand(Guid.NewGuid(), Guid.NewGuid(), null, null, null, 0, new List<CreatePurchaseOrderItemDto>()); // List rỗng

        _purchaseOrderRepoMock.Setup(x => x.SupplierExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Purchase order must have at least one item.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStoreIdIsNull()
    {
        // Arrange
        var validItems = new List<CreatePurchaseOrderItemDto> { new(Guid.NewGuid(), 50) };
        var command = new CreatePurchaseOrderCommand(Guid.NewGuid(), null, null, null, null, 0, validItems); // StoreId rỗng

        _purchaseOrderRepoMock.Setup(x => x.SupplierExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("StoreId is required.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenStoreDoesNotExistInFranchiseService()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var validItems = new List<CreatePurchaseOrderItemDto> { new(Guid.NewGuid(), 50) };
        var command = new CreatePurchaseOrderCommand(Guid.NewGuid(), storeId, null, null, null, 0, validItems);

        _purchaseOrderRepoMock.Setup(x => x.SupplierExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _franchiseClientMock.Setup(x => x.GetFranchiseByIdAsync(storeId)).ReturnsAsync(false); // Kho không tồn tại

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Store {storeId} does not exist.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenIngredientPricesAreMissingFromSupplier()
    {
        // Arrange
        var ingredient1 = Guid.NewGuid(); // Hạt Arabica Cầu Đất
        var ingredient2 = Guid.NewGuid(); // Hạt Robusta Đắk Lắk (Chưa có giá trong mapping của nhà cung cấp này)

        var items = new List<CreatePurchaseOrderItemDto>
        {
            new(ingredient1, 10),
            new(ingredient2, 20)
        };
        var command = new CreatePurchaseOrderCommand(Guid.NewGuid(), Guid.NewGuid(), null, null, null, 0, items);

        _purchaseOrderRepoMock.Setup(x => x.SupplierExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _franchiseClientMock.Setup(x => x.GetFranchiseByIdAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        // Mock DB chỉ trả về giá của thằng Ingredient1, thiếu Ingredient2
        var priceMap = new Dictionary<Guid, decimal> { { ingredient1, 150000 } };
        _purchaseOrderRepoMock
            .Setup(x => x.GetSupplierIngredientPricesAsync(command.SupplierId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceMap);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Ingredients not found for this supplier: {ingredient2}");
    }

    #endregion 1. Validation Tests (Các nhánh văng Exception)

    #region 2. Happy Path (Luồng Thành Công & Tính Toán)

    [Fact]
    public async Task Handle_ShouldCreatePurchaseOrder_AndCalculateTotalCorrectly_WhenAllValid()
    {
        // Arrange
        var storeId = Guid.NewGuid(); // Chi nhánh Quận 1
        var supplierId = Guid.NewGuid(); // Nhà cung cấp Nguyên liệu Cà phê

        var arabicaId = Guid.NewGuid();
        var suaDacId = Guid.NewGuid();

        var discount = 50000m; // Giảm giá 50k

        var items = new List<CreatePurchaseOrderItemDto>
        {
            new(arabicaId, 10m), // 10kg Arabica
            new(suaDacId, 50m)   // 50 hộp Sữa đặc
        };

        var command = new CreatePurchaseOrderCommand(
            supplierId, storeId, "Nhập gấp cho event cuối tuần", "REF-123", DateTimeOffset.UtcNow.AddDays(2), discount, items);

        _purchaseOrderRepoMock.Setup(x => x.SupplierExistsAsync(supplierId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _franchiseClientMock.Setup(x => x.GetFranchiseByIdAsync(storeId)).ReturnsAsync(true);

        // Set giá: Arabica 150k/kg, Sữa đặc 25k/hộp
        var priceMap = new Dictionary<Guid, decimal>
        {
            { arabicaId, 150000m },
            { suaDacId, 25000m }
        };

        _purchaseOrderRepoMock
            .Setup(x => x.GetSupplierIngredientPricesAsync(supplierId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceMap);

        var expectedCode = "PO-20260328-001";
        _purchaseOrderRepoMock.Setup(x => x.GenerateNextCodeAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expectedCode);

        // Dùng CallBack để chộp lấy đối tượng PurchaseOrder thực tế mà Handler đang Build
        PurchaseOrder? savedPo = null;
        _purchaseOrderRepoMock
            .Setup(x => x.AddAsync(It.IsAny<PurchaseOrder>(), It.IsAny<CancellationToken>()))
            .Callback<PurchaseOrder, CancellationToken>((po, _) => savedPo = po);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - 1. Kiểm tra Result trả về
        result.Should().NotBeNull();
        result.Code.Should().Be(expectedCode);

        // Assert - 2. Kiểm tra các Method đã được gọi chuẩn xác chưa
        _purchaseOrderRepoMock.Verify(x => x.AddAsync(It.IsAny<PurchaseOrder>(), It.IsAny<CancellationToken>()), Times.Once);
        _purchaseOrderRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Assert - 3. Kiểm tra tính toán Logic của Entity
        savedPo.Should().NotBeNull();
        savedPo!.SupplierId.Should().Be(supplierId);
        savedPo.StoreId.Should().Be(storeId);
        savedPo.Status.Should().Be(PurchaseOrderStatus.Pending);
        savedPo.DiscountAmount.Should().Be(discount);

        // Tính toán: (10 * 150.000) + (50 * 25.000) = 1.500.000 + 1.250.000 = 2.750.000
        var expectedTotalAmount = 2750000m;
        savedPo.TotalAmount.Should().Be(expectedTotalAmount);

        // NetAmount = 2.750.000 - 50.000 = 2.700.000
        var expectedNetAmount = 2700000m;
        savedPo.NetAmount.Should().Be(expectedNetAmount);

        // Kiểm tra Mapping Item
        savedPo.Items.Should().HaveCount(2);
        savedPo.Items.First(i => i.IngredientId == arabicaId).TotalPrice.Should().Be(1500000m);
        savedPo.Items.First(i => i.IngredientId == suaDacId).TotalPrice.Should().Be(1250000m);
    }

    #endregion 2. Happy Path (Luồng Thành Công & Tính Toán)
}