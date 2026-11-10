namespace WarehouseService.Application.Tests.Features.Warehouses;

using FluentAssertions;
using Moq;
using WarehouseService.Application.ClientInterfaces;
using WarehouseService.Application.Features.Warehouses.Command.CreateWarehouse;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class CreateWarehouseTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IFranchiseServiceClient> _franchiseServiceClientMock;
    private readonly CreateWarehouseCommandHandler _handler;

    public CreateWarehouseTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _franchiseServiceClientMock = new Mock<IFranchiseServiceClient>();

        _handler = new CreateWarehouseCommandHandler(
            _warehouseRepositoryMock.Object,
            _franchiseServiceClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenStoreDoesNotExist()
    {
        var storeId = Guid.NewGuid();
        var command = CreateValidCommand(storeId, "WH-Q1-01", "123 Lê Lợi, Quận 1");

        _franchiseServiceClientMock
            .Setup(x => x.StoreExistsAsync(storeId))
            .ReturnsAsync(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Store with Id '{storeId}' not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenWarehouseAlreadyExistsForStore()
    {
        var storeId = Guid.NewGuid();
        var command = CreateValidCommand(storeId, "WH-Q1-02", "456 Nguyễn Huệ, Quận 1");

        _franchiseServiceClientMock.Setup(x => x.StoreExistsAsync(storeId)).ReturnsAsync(true);
        _warehouseRepositoryMock
            .Setup(x => x.ExistWithStoreId(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Warehouse with Store Id '{storeId}' already existed.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenWarehouseCodeAlreadyExists()
    {
        var storeId = Guid.NewGuid();
        var code = "WH-ROBUSTA-DAKLAK";
        var command = CreateValidCommand(storeId, code, "Thôn 1, Xã Ea Knuếc, Đắk Lắk");

        _franchiseServiceClientMock.Setup(x => x.StoreExistsAsync(storeId)).ReturnsAsync(true);
        _warehouseRepositoryMock.Setup(x => x.ExistWithStoreId(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _warehouseRepositoryMock
            .Setup(x => x.CodeExistsAsync(code, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Warehouse Code '{code}' already existed.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenAddressAlreadyExists()
    {
        var storeId = Guid.NewGuid();
        var address = "789 Phan Xích Long, Phú Nhuận";
        var command = CreateValidCommand(storeId, "WH-PN-01", address);

        _franchiseServiceClientMock.Setup(x => x.StoreExistsAsync(storeId)).ReturnsAsync(true);
        _warehouseRepositoryMock.Setup(x => x.ExistWithStoreId(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _warehouseRepositoryMock.Setup(x => x.CodeExistsAsync(It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(false);

        _warehouseRepositoryMock
            .Setup(x => x.ExistWithStoreAddress(address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Warehouse with Address '{address}' already existed.");
    }

    [Fact]
    public async Task Handle_ShouldCreateWarehouse_WhenAllValidationsPass()
    {
        var storeId = Guid.NewGuid();
        var command = new CreateWarehouseCommand(
            " wh-arabica-saigon ", // Test logic Trim và UpperCase
            " Kho Cà Phê Arabica Sài Gòn ",
            " 123 Tú Xương, Quận 3 ",
            storeId,
            WarehouseStatus.Active);

        _franchiseServiceClientMock.Setup(x => x.StoreExistsAsync(storeId)).ReturnsAsync(true);
        _warehouseRepositoryMock.Setup(x => x.ExistWithStoreId(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _warehouseRepositoryMock.Setup(x => x.CodeExistsAsync(It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(false);

        _warehouseRepositoryMock.Setup(x => x.ExistWithStoreAddress(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be("WH-ARABICA-SAIGON");
        result.Name.Should().Be("Kho Cà Phê Arabica Sài Gòn");
        result.Address.Should().Be("123 Tú Xương, Quận 3");
        result.Status.Should().Be(WarehouseStatus.Active);

        _warehouseRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()), Times.Once);
        _warehouseRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CreateWarehouseCommand CreateValidCommand(Guid storeId, string code, string address)
    {
        return new CreateWarehouseCommand(
            code,
            "Kho Cà Phê Mẫu",
            address,
            storeId,
            WarehouseStatus.Active);
    }
}