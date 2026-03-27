using Moq;
using Xunit;
using FluentAssertions;
using WarehouseService.Application.Features.WarehouseItems.Query.GetById;
using WarehouseService.Application.Repositories;

namespace WarehouseService.Application.Tests.Features.WarehouseItems;

public class GetWarehouseItemQueryHandlerTests
{
    private readonly Mock<IWarehouseItemRepository> _itemRepositoryMock;
    private readonly GetWarehouseItemQueryHandler _handler;

    public GetWarehouseItemQueryHandlerTests()
    {
        _itemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _handler = new GetWarehouseItemQueryHandler(_itemRepositoryMock.Object);
    }

    // -----------------------------------------------------------------------
    // GROUP 1 — NOT FOUND BRANCH
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenItemDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.WarehouseItem?)null);

        var query = new GetWarehouseItemQuery(id);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(query, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — FOUND BRANCH: MAPPING
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnResult_WithAllFieldsMapped_WhenItemExists()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var expiry = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var createdAt = DateTimeOffset.UtcNow.AddDays(-10);
        var updatedAt = DateTimeOffset.UtcNow.AddDays(-1);

        var item = new Domain.Entities.WarehouseItem
        {
            Id = Guid.NewGuid(),
            WarehouseId = warehouseId,
            IngredientId = ingredientId,
            Unit = "kg",
            Quantity = 7.5m,
            BasePrice = 12.99m,
            ExpiryDate = expiry,
            IsDeleted = false,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var query = new GetWarehouseItemQuery(item.Id);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(item.Id);
        result.WarehouseId.Should().Be(warehouseId);
        result.IngredientId.Should().Be(ingredientId);
        result.Unit.Should().Be("kg");
        result.Quantity.Should().Be(7.5m);
        result.BasePrice.Should().Be(12.99m);
        result.ExpiryDate.Should().Be(expiry);
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task Handle_ShouldReturnNullExpiryDate_WhenItemHasNoExpiry()
    {
        // Arrange
        var item = new Domain.Entities.WarehouseItem
        {
            Id = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            IngredientId = Guid.NewGuid(),
            Unit = "pcs",
            Quantity = 10m,
            BasePrice = 5m,
            ExpiryDate = null,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var query = new GetWarehouseItemQuery(item.Id);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.ExpiryDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallGetByIdAsync_WithCorrectId()
    {
        // Arrange
        var id = Guid.NewGuid();

        var item = new Domain.Entities.WarehouseItem
        {
            Id = id,
            WarehouseId = Guid.NewGuid(),
            IngredientId = Guid.NewGuid(),
            Unit = "kg",
            Quantity = 1m,
            BasePrice = 1m,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var query = new GetWarehouseItemQuery(id);

        // Act
        await _handler.Handle(query, default);

        // Assert
        _itemRepositoryMock.Verify(
            x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}