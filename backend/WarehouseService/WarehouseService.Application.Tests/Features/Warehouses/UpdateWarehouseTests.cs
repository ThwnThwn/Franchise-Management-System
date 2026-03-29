namespace WarehouseService.Application.Tests.Features.Warehouses;

using FluentAssertions;
using Moq;
using WarehouseService.Application.ClientInterfaces;
using WarehouseService.Application.Features.Warehouses.Command.UpdateWarehouse;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class UpdateWarehouseTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IFranchiseServiceClient> _franchiseServiceClientMock;
    private readonly UpdateWarehouseCommandHandler _handler;

    public UpdateWarehouseTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _franchiseServiceClientMock = new Mock<IFranchiseServiceClient>();

        _handler = new UpdateWarehouseCommandHandler(
            _warehouseRepositoryMock.Object,
            _franchiseServiceClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenWarehouseNotFound()
    {
        var warehouseId = Guid.NewGuid();
        var command = new UpdateWarehouseCommand(warehouseId, "WH-NEW", null, null, null, null);

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Warehouse with ID '{warehouseId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNewCodeAlreadyExists()
    {
        var warehouseId = Guid.NewGuid();
        var existingCode = "WH-ROBUSTA-SANG-TAO";
        var command = new UpdateWarehouseCommand(warehouseId, existingCode, null, null, null, null);
        var warehouse = new Warehouse { Id = warehouseId, Code = "OLD-CODE" };

        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);
        _warehouseRepositoryMock
            .Setup(x => x.CodeExistsAsync(existingCode, warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Warehouse code '{existingCode}' already exists.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNewAddressAlreadyExists()
    {
        var warehouseId = Guid.NewGuid();
        var duplicateAddress = "Số 10, Đường Arabica, Cầu Đất";
        var command = new UpdateWarehouseCommand(warehouseId, null, null, duplicateAddress, null, null);
        var warehouse = new Warehouse { Id = warehouseId, Address = "Địa chỉ cũ" };

        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);
        _warehouseRepositoryMock
            .Setup(x => x.ExistWithStoreAddress(duplicateAddress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Warehouse with address '{duplicateAddress}' already exists.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenNewStoreDoesNotExist()
    {
        var warehouseId = Guid.NewGuid();
        var newStoreId = Guid.NewGuid();
        var command = new UpdateWarehouseCommand(warehouseId, null, null, null, null, newStoreId);
        var warehouse = new Warehouse { Id = warehouseId, StoreId = Guid.NewGuid() };

        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);
        _franchiseServiceClientMock.Setup(x => x.StoreExistsAsync(newStoreId)).ReturnsAsync(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Store with ID '{newStoreId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNewStoreAlreadyHasAWarehouse()
    {
        var warehouseId = Guid.NewGuid();
        var newStoreId = Guid.NewGuid();
        var command = new UpdateWarehouseCommand(warehouseId, null, null, null, null, newStoreId);
        var warehouse = new Warehouse { Id = warehouseId, StoreId = Guid.NewGuid() };

        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);
        _franchiseServiceClientMock.Setup(x => x.StoreExistsAsync(newStoreId)).ReturnsAsync(true);
        _warehouseRepositoryMock
            .Setup(x => x.ExistWithStoreId(newStoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"A warehouse with store ID '{newStoreId}' already exists.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateAllFieldsSuccessfully_WhenDataIsValid()
    {
        var warehouseId = Guid.NewGuid();
        var newStoreId = Guid.NewGuid();
        var command = new UpdateWarehouseCommand(
            warehouseId,
            " wh-moka-premium ",
            " Tổng Kho Moka Thượng Hạng ",
            " 99 Phan Xích Long, Phú Nhuận ",
            WarehouseStatus.UnderMaintenance,
            newStoreId);

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            Code = "OLD-CODE",
            StoreId = Guid.NewGuid(),
            WarehouseItems = [],
            StockTransactions = [],
            TransfersIn = [],
            TransfersOut = []
        };

        _warehouseRepositoryMock.Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);
        _warehouseRepositoryMock.Setup(x => x.CodeExistsAsync(It.IsAny<string>(), warehouseId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _warehouseRepositoryMock.Setup(x => x.ExistWithStoreAddress(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _franchiseServiceClientMock.Setup(x => x.StoreExistsAsync(newStoreId)).ReturnsAsync(true);
        _warehouseRepositoryMock.Setup(x => x.ExistWithStoreId(newStoreId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("WH-MOKA-PREMIUM");
        result.Name.Should().Be("Tổng Kho Moka Thượng Hạng");
        result.Address.Should().Be("99 Phan Xích Long, Phú Nhuận");
        result.Status.Should().Be(WarehouseStatus.UnderMaintenance);
        result.StoreId.Should().Be(newStoreId);

        _warehouseRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkipStoreValidation_WhenNewStoreIdIsSameAsCurrent()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var currentStoreId = Guid.NewGuid(); // StoreId hiện tại của kho

        // Request gửi lên StoreId y chang cái đang có
        var command = new UpdateWarehouseCommand(
            warehouseId,
            null,
            null,
            null,
            null,
            currentStoreId // Cùng StoreId
        );

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            StoreId = currentStoreId, // Cùng StoreId
            Code = "WH-ROBUSTA",
            Name = "Kho Robusta",
            Address = "Gia Lai",
            WarehouseItems = [],
            StockTransactions = []
        };

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.StoreId.Should().Be(currentStoreId);

        // Cực kỳ quan trọng: Phải verify rẳng FranchiseServiceClient KHÔNG bị gọi
        _franchiseServiceClientMock.Verify(x => x.StoreExistsAsync(It.IsAny<Guid>()), Times.Never);

        // Verify rằng hàm check trùng StoreId cũng KHÔNG bị gọi
        _warehouseRepositoryMock.Verify(x => x.ExistWithStoreId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

        // Nhưng cuối cùng vẫn lưu (chỉ UpdateUpdatedAt)
        _warehouseRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotUpdateStoreId_WhenStoreIdIsNull()
    {
        // Arrange: Khởi tạo kho gốc với StoreId có sẵn
        var warehouseId = Guid.NewGuid();
        var originalStoreId = Guid.NewGuid();

        // Cố tình truyền StoreId = null, chỉ cập nhật Name
        var command = new UpdateWarehouseCommand(
            warehouseId,
            null,
            "Tổng Kho Arabica Cầu Đất", // Đổi tên kho
            null,
            null,
            null // Điểm mấu chốt: StoreId = null để cover nhánh A sai (HasValue == false)
        );

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            StoreId = originalStoreId,
            Code = "WH-ARABICA",
            Name = "Kho Arabica Cũ",
            Address = "Đà Lạt",
            WarehouseItems = [],
            StockTransactions = []
        };

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Tổng Kho Arabica Cầu Đất"); // Đảm bảo đã update
        result.StoreId.Should().Be(originalStoreId); // Đảm bảo StoreId giữ nguyên

        // Đảm bảo không gọi sang các service khác
        _franchiseServiceClientMock.Verify(x => x.StoreExistsAsync(It.IsAny<Guid>()), Times.Never);
        _warehouseRepositoryMock.Verify(x => x.ExistWithStoreId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

        // Vẫn lưu xuống DB
        _warehouseRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}