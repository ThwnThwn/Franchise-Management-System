namespace WarehouseService.Application.Tests.Features.WarehouseItems;

using FluentAssertions;
using Moq;
using WarehouseService.Application.ClientInterfaces;
using WarehouseService.Application.Features.WarehouseItems.Command.CreateWarehouseItem;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;

public class CreateWarehouseItemTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly Mock<IInventoryServiceClient> _inventoryServiceClientMock;
    private readonly CreateWarehouseItemCommandHandler _handler;

    public CreateWarehouseItemTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _inventoryServiceClientMock = new Mock<IInventoryServiceClient>();

        _handler = new CreateWarehouseItemCommandHandler(
            _warehouseRepositoryMock.Object,
            _warehouseItemRepositoryMock.Object,
            _inventoryServiceClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenWarehouseIsNotFound()
    {
        var warehouseId = Guid.NewGuid();
        var command = CreateValidCommand(warehouseId, Guid.NewGuid());

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Warehouse with ID '{warehouseId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenIngredientDoesNotExistInSystem()
    {
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid(); // ID của "Bột Cacao Nguyên Chất"
        var command = CreateValidCommand(warehouseId, ingredientId);

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = warehouseId });

        _inventoryServiceClientMock
            .Setup(x => x.IngredientExistsAsync(ingredientId))
            .ReturnsAsync(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Ingredient with ID '{ingredientId}' does not exist.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenIngredientAlreadyInWarehouse()
    {
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid(); // ID của "Sữa Đặc Larosee"
        var command = CreateValidCommand(warehouseId, ingredientId);

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = warehouseId });

        _inventoryServiceClientMock
            .Setup(x => x.IngredientExistsAsync(ingredientId))
            .ReturnsAsync(true);

        _warehouseItemRepositoryMock
            .Setup(x => x.IngredientExistsInWarehouseAsync(warehouseId, ingredientId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Ingredient with ID '{ingredientId}' already exists in this warehouse.");
    }

    // =====================================================================
    // CASE MỚI THÊM ĐỂ COVER NHÁNH EXPIREDATE = NULL (Dụng cụ không có HSD)
    // =====================================================================
    [Fact]
    public async Task Handle_ShouldCreateWarehouseItem_WhenExpiryDateIsNull()
    {
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid(); // Ly thủy tinh (Không có HSD)

        // Truyền ExpiryDate = null
        var command = new CreateWarehouseItemCommand(
            warehouseId,
            ingredientId,
            "Cái",
            100,
            15000,
            null);

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = warehouseId, Name = "Kho Vật Tư" });

        _inventoryServiceClientMock
            .Setup(x => x.IngredientExistsAsync(ingredientId))
            .ReturnsAsync(true);

        _warehouseItemRepositoryMock
            .Setup(x => x.IngredientExistsInWarehouseAsync(warehouseId, ingredientId,
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.ExpiryDate.Should().BeNull(); // Đảm bảo null được pass qua

        _warehouseItemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<WarehouseItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _warehouseItemRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // =====================================================================

    [Fact]
    public async Task Handle_ShouldCreateWarehouseItem_WhenAllValidationsPass()
    {
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid(); // ID của "Hạt Cà Phê Moka"
        var command = new CreateWarehouseItemCommand(
            warehouseId,
            ingredientId,
            " Túi 500g ", // Test bóc tách Trim()
            10,
            125000,
            DateTime.UtcNow.AddMonths(6));

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = warehouseId, Name = "Kho Nguyên Liệu Quận 3" });

        _inventoryServiceClientMock
            .Setup(x => x.IngredientExistsAsync(ingredientId))
            .ReturnsAsync(true);

        _warehouseItemRepositoryMock
            .Setup(x => x.IngredientExistsInWarehouseAsync(warehouseId, ingredientId,
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Unit.Should().Be("Túi 500g");
        result.Quantity.Should().Be(10);

        _warehouseItemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<WarehouseItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _warehouseItemRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapPropertiesCorrectly_AndTrimUnitString()
    {
        // Kịch bản: Nhập Trà Ô Long vào kho, cố tình truyền Unit có khoảng trắng thừa
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var expiryDate = DateTime.UtcNow.AddMonths(6);

        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = warehouseId });

        _inventoryServiceClientMock.Setup(x => x.IngredientExistsAsync(ingredientId))
            .ReturnsAsync(true);

        _warehouseItemRepositoryMock.Setup(x => x.IngredientExistsInWarehouseAsync(
            warehouseId, ingredientId, null,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // CỐ TÌNH TRUYỀN UNIT: "  Gói  "
        var command = new CreateWarehouseItemCommand(warehouseId, ingredientId, "  Gói  ", 150, 85000, expiryDate);

        // Kỹ thuật Callback: Chộp lấy entity ngay trước khi nó được ném vào DB
        WarehouseItem? savedItem = null;
        _warehouseItemRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<WarehouseItem>(), It.IsAny<CancellationToken>()))
            .Callback<WarehouseItem, CancellationToken>((item, _) => savedItem = item)
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        // Assert: Quét sạch mọi ngóc ngách của khối khởi tạo WarehouseItem
        savedItem.Should().NotBeNull();
        savedItem!.Id.Should().NotBeEmpty();
        savedItem.WarehouseId.Should().Be(warehouseId);
        savedItem.IngredientId.Should().Be(ingredientId);

        // Đảm bảo hàm .Trim() đã chạy thành công
        savedItem.Unit.Should().Be("Gói");

        savedItem.Quantity.Should().Be(150);
        savedItem.BasePrice.Should().Be(85000);
        savedItem.ExpiryDate.Should().Be(DateTime.SpecifyKind(expiryDate, DateTimeKind.Utc));
        savedItem.IsDeleted.Should().BeFalse();

        // Kiểm tra thời gian khởi tạo (chênh lệch tối đa 1 giây)
        savedItem.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        savedItem.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    private static CreateWarehouseItemCommand CreateValidCommand(Guid warehouseId, Guid ingredientId)
    {
        return new CreateWarehouseItemCommand(
            warehouseId,
            ingredientId,
            "Lon",
            0,
            25000,
            null);
    }
}