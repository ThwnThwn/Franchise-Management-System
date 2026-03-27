using Moq;
using Xunit;
using FluentAssertions;
using WarehouseService.Application.Features.Warehouses.Command.CreateWarehouse;
using WarehouseService.Application.Repositories;
using WarehouseService.Application.ClientInterfaces;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Tests.Features.Warehouses;

public class CreateWarehouseHandlerTests
{
    private readonly Mock<IWarehouseRepository> _repositoryMock;
    private readonly Mock<IFranchiseServiceClient> _franchiseClientMock;
    private readonly CreateWarehouseCommandHandler _handler;

    public CreateWarehouseHandlerTests()
    {
        _repositoryMock = new Mock<IWarehouseRepository>();
        _franchiseClientMock = new Mock<IFranchiseServiceClient>();

        _handler = new CreateWarehouseCommandHandler(
            _repositoryMock.Object,
            _franchiseClientMock.Object
        );

        // Default: store exists, nothing conflicts
        _franchiseClientMock
            .Setup(x => x.StoreExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.ExistWithStoreId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.CodeExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.ExistWithStoreAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Warehouse>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
    }

    // -----------------------------------------------------------------------
    // GROUP 1 — FRANCHISE / STORE VALIDATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenStoreDoesNotExist()
    {
        // Arrange
        var storeId = Guid.NewGuid();

        _franchiseClientMock
            .Setup(x => x.StoreExistsAsync(storeId))
            .ReturnsAsync(false);

        var command = BuildCommand(storeId: storeId);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{storeId}*");
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — DUPLICATE / CONFLICT VALIDATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenWarehouseAlreadyExistsForStore()
    {
        // Arrange
        var storeId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.ExistWithStoreId(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand(storeId: storeId);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{storeId}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenCodeAlreadyExists()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.CodeExistsAsync("WH-001", It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand(code: "WH-001");

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*WH-001*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenAddressAlreadyExists()
    {
        // Arrange
        const string address = "123 Main St";

        _repositoryMock
            .Setup(x => x.ExistWithStoreAddress(address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand(address: address);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{address}*");
    }

    // -----------------------------------------------------------------------
    // GROUP 3 — SUCCESSFUL CREATION & FIELD MAPPING
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnResult_WithCorrectMappedFields_WhenRequestIsValid()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var command = BuildCommand(
            code: "wh-abc",
            name: "  Main Warehouse  ",
            address: "  456 Elm Street  ",
            storeId: storeId,
            status: WarehouseStatus.Active
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Code.Should().Be("WH-ABC");                // ToUpper + Trim
        result.Name.Should().Be("Main Warehouse");         // Trim
        result.Address.Should().Be("456 Elm Street");      // Trim
        result.StoreId.Should().Be(storeId);
        result.Status.Should().Be(WarehouseStatus.Active);
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldNormaliseCodeToUpperCase_WhenCodeIsLowerCase()
    {
        // Arrange
        var command = BuildCommand(code: "wh-south");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Code.Should().Be("WH-SOUTH");
    }

    [Fact]
    public async Task Handle_ShouldTrimWhitespace_FromCodeNameAndAddress()
    {
        // Arrange
        var command = BuildCommand(
            code: "  WH-002  ",
            name: "  South Hub  ",
            address: "  789 Oak Ave  "
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Code.Should().Be("WH-002");
        result.Name.Should().Be("South Hub");
        result.Address.Should().Be("789 Oak Ave");
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — REPOSITORY INTERACTION VERIFICATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCallAddAsync_ExactlyOnce_WhenRequestIsValid()
    {
        // Arrange
        var command = BuildCommand();

        // Act
        await _handler.Handle(command, default);

        // Assert
        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Warehouse>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync_ExactlyOnce_WhenRequestIsValid()
    {
        // Arrange
        var command = BuildCommand();

        // Act
        await _handler.Handle(command, default);

        // Assert
        _repositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldNotCallAddAsync_WhenStoreDoesNotExist()
    {
        // Arrange
        _franchiseClientMock
            .Setup(x => x.StoreExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(false);

        var command = BuildCommand();

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Warehouse>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldNotCallAddAsync_WhenCodeAlreadyExists()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.CodeExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand();

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Warehouse>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 5 — VALIDATION ORDER (guards fire in the right sequence)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCheckStoreExistence_BeforeCheckingWarehouseConflicts()
    {
        // Arrange — store does not exist, but we also set up a storeId conflict.
        //           Only the store-not-found guard should fire.
        var storeId = Guid.NewGuid();

        _franchiseClientMock
            .Setup(x => x.StoreExistsAsync(storeId))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.ExistWithStoreId(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand(storeId: storeId);

        // Act & Assert — only KeyNotFoundException, not InvalidOperationException
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    // -----------------------------------------------------------------------
    // HELPERS
    // -----------------------------------------------------------------------

    private static CreateWarehouseCommand BuildCommand(
        string? code = null,
        string? name = null,
        string? address = null,
        Guid? storeId = null,
        WarehouseStatus? status = null
    ) => new CreateWarehouseCommand(
        Code: code ?? "WH-DEFAULT",
        Name: name ?? "Default Warehouse",
        Address: address ?? "1 Default Street",
        StoreId: storeId ?? Guid.NewGuid(),
        Status: status ?? WarehouseStatus.Active
    );
}