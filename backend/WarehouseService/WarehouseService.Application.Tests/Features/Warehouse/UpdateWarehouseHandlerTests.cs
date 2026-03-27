using Moq;
using Xunit;
using FluentAssertions;
using WarehouseService.Application.Features.Warehouses.Command.UpdateWarehouse;
using WarehouseService.Application.Repositories;
using WarehouseService.Application.ClientInterfaces;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Tests.Features.Warehouses;

public class UpdateWarehouseHandlerTests
{
    private readonly Mock<IWarehouseRepository> _repositoryMock;
    private readonly Mock<IFranchiseServiceClient> _franchiseClientMock;
    private readonly UpdateWarehouseCommandHandler _handler;

    // A reusable warehouse entity returned by GetByIdAsync in the happy path.
    private readonly Domain.Entities.Warehouse _existingWarehouse;

    public UpdateWarehouseHandlerTests()
    {
        _repositoryMock = new Mock<IWarehouseRepository>();
        _franchiseClientMock = new Mock<IFranchiseServiceClient>();

        _handler = new UpdateWarehouseCommandHandler(
            _repositoryMock.Object,
            _franchiseClientMock.Object
        );

        _existingWarehouse = new Domain.Entities.Warehouse
        {
            Id = Guid.NewGuid(),
            Code = "WH-EXISTING",
            Name = "Existing Warehouse",
            Address = "1 Existing Street",
            StoreId = Guid.NewGuid(),
            Status = WarehouseStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        // Default: warehouse found, no conflicts, store exists
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingWarehouse);

        _repositoryMock
            .Setup(x => x.CodeExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.ExistWithStoreAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.ExistWithStoreId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _franchiseClientMock
            .Setup(x => x.StoreExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);
    }

    // -----------------------------------------------------------------------
    // GROUP 1 — WAREHOUSE NOT FOUND
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenWarehouseDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Warehouse?)null);

        var command = BuildCommand(id: id);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenWarehouseDoesNotExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Warehouse?)null);

        var command = BuildCommand();

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _repositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — CODE CONFLICT VALIDATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNewCodeAlreadyExists()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.CodeExistsAsync("WH-TAKEN", It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand(code: "WH-TAKEN");

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*WH-TAKEN*");
    }

    [Fact]
    public async Task Handle_ShouldPassExcludeId_ToCodeExistsCheck_SoWarehouseCanKeepItsOwnCode()
    {
        // Arrange — CodeExistsAsync must be called with the warehouse's own Id as excludeId,
        //           so updating with the same code does not self-conflict.
        var command = BuildCommand(code: "WH-EXISTING");

        // Act
        await _handler.Handle(command, default);

        // Assert — excludeId == warehouse.Id
        _repositoryMock.Verify(
            x => x.CodeExistsAsync(
                It.IsAny<string>(),
                _existingWarehouse.Id,          // excludeId must be the entity's own Id
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldSkipCodeCheck_WhenCodeIsNullOrWhiteSpace()
    {
        // Arrange
        var command = BuildCommand(code: null);

        // Act
        await _handler.Handle(command, default);

        // Assert — CodeExistsAsync must never be called when code is not provided
        _repositoryMock.Verify(
            x => x.CodeExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 3 — ADDRESS CONFLICT VALIDATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNewAddressAlreadyExists()
    {
        // Arrange
        const string takenAddress = "99 Taken Ave";

        _repositoryMock
            .Setup(x => x.ExistWithStoreAddress(takenAddress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand(address: takenAddress);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{takenAddress}*");
    }

    [Fact]
    public async Task Handle_ShouldSkipAddressCheck_WhenAddressIsNullOrWhiteSpace()
    {
        // Arrange
        var command = BuildCommand(address: null);

        // Act
        await _handler.Handle(command, default);

        // Assert
        _repositoryMock.Verify(
            x => x.ExistWithStoreAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — STORE CHANGE VALIDATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenNewStoreDoesNotExist()
    {
        // Arrange
        var newStoreId = Guid.NewGuid(); // different from _existingWarehouse.StoreId

        _franchiseClientMock
            .Setup(x => x.StoreExistsAsync(newStoreId))
            .ReturnsAsync(false);

        var command = BuildCommand(storeId: newStoreId);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{newStoreId}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNewStoreAlreadyHasAWarehouse()
    {
        // Arrange
        var newStoreId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.ExistWithStoreId(newStoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand(storeId: newStoreId);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{newStoreId}*");
    }

    [Fact]
    public async Task Handle_ShouldSkipStoreChecks_WhenStoreIdIsNull()
    {
        // Arrange
        var command = BuildCommand(storeId: null);

        // Act
        await _handler.Handle(command, default);

        // Assert — no franchise call, no storeId conflict check
        _franchiseClientMock.Verify(
            x => x.StoreExistsAsync(It.IsAny<Guid>()),
            Times.Never
        );

        _repositoryMock.Verify(
            x => x.ExistWithStoreId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldSkipStoreChecks_WhenStoreIdIsUnchanged()
    {
        // Arrange — pass the same StoreId that is already on the warehouse
        var command = BuildCommand(storeId: _existingWarehouse.StoreId);

        // Act
        await _handler.Handle(command, default);

        // Assert — handler short-circuits when StoreId hasn't changed
        _franchiseClientMock.Verify(
            x => x.StoreExistsAsync(It.IsAny<Guid>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 5 — SUCCESSFUL UPDATE & FIELD MAPPING
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnUpdatedResult_WithAllFieldsMapped_WhenRequestIsValid()
    {
        // Arrange
        var newStoreId = Guid.NewGuid();
        var command = BuildCommand(
            code: "wh-new",
            name: "  Updated Warehouse  ",
            address: "  10 New Street  ",
            status: WarehouseStatus.Inactive,
            storeId: newStoreId
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_existingWarehouse.Id);
        result.Code.Should().Be("WH-NEW");               // ToUpper + Trim
        result.Name.Should().Be("Updated Warehouse");     // Trim
        result.Address.Should().Be("10 New Street");      // Trim
        result.Status.Should().Be(WarehouseStatus.Inactive);
        result.StoreId.Should().Be(newStoreId);
        result.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldNormaliseCodeToUpperCase_WhenCodeIsLowerCase()
    {
        // Arrange
        var command = BuildCommand(code: "wh-updated");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Code.Should().Be("WH-UPDATED");
    }

    [Fact]
    public async Task Handle_ShouldTrimWhitespace_FromCodeNameAndAddress()
    {
        // Arrange
        var command = BuildCommand(
            code: "  WH-TRIM  ",
            name: "  Trimmed Name  ",
            address: "  Trimmed Address  "
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Code.Should().Be("WH-TRIM");
        result.Name.Should().Be("Trimmed Name");
        result.Address.Should().Be("Trimmed Address");
    }

    [Fact]
    public async Task Handle_ShouldPreserveExistingFields_WhenNullableFieldsAreNotProvided()
    {
        // Arrange — only update Name; Code, Address, Status, StoreId are all null
        var command = BuildCommand(
            code: null,
            name: "Only Name Changed",
            address: null,
            status: null,
            storeId: null
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert — unchanged fields come from the existing entity
        result.Code.Should().Be(_existingWarehouse.Code);
        result.Name.Should().Be("Only Name Changed");
        result.Address.Should().Be(_existingWarehouse.Address);
        result.Status.Should().Be(_existingWarehouse.Status);
        result.StoreId.Should().Be(_existingWarehouse.StoreId);
    }

    // -----------------------------------------------------------------------
    // GROUP 6 — REPOSITORY INTERACTION VERIFICATION
    // -----------------------------------------------------------------------

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
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenCodeConflictIsDetected()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.CodeExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand(code: "WH-CONFLICT");

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _repositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenAddressConflictIsDetected()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ExistWithStoreAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = BuildCommand(address: "Taken Address");

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _repositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // HELPERS
    // -----------------------------------------------------------------------

    private static UpdateWarehouseCommand BuildCommand(
        Guid? id = null,
        string? code = "WH-UPDATED",
        string? name = "Updated Warehouse",
        string? address = "2 Updated Street",
        WarehouseStatus? status = WarehouseStatus.Active,
        Guid? storeId = null   // null triggers no-store-change path by default
    ) => new UpdateWarehouseCommand(
        Id: id ?? Guid.NewGuid(),
        Code: code,
        Name: name,
        Address: address,
        Status: status,
        StoreId: storeId
    );
}