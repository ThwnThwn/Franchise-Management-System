namespace WarehouseService.Application.Tests.Features.WarehouseItems;

using FluentAssertions;
using Moq;
using WarehouseService.Application.Features.WarehouseItems.Query.GetById;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;

public class GetWarehouseItemTests
{
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly GetWarehouseItemQueryHandler _handler;

    public GetWarehouseItemTests()
    {
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _handler = new GetWarehouseItemQueryHandler(_warehouseItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnWarehouseItem_WhenIdExists()
    {
        var itemId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid(); // ID của "Hạt Cà Phê Robusta Rang Củi"
        var warehouseId = Guid.NewGuid();

        var item = new WarehouseItem
        {
            Id = itemId,
            WarehouseId = warehouseId,
            IngredientId = ingredientId,
            Unit = "Bao 5kg",
            Quantity = 20,
            BasePrice = 450000,
            ExpiryDate = DateTime.UtcNow.AddMonths(18),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            StockTransactions = []
        };

        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var query = new GetWarehouseItemQuery(itemId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(itemId);
        result.IngredientId.Should().Be(ingredientId);
        result.Unit.Should().Be("Bao 5kg");
        result.Quantity.Should().Be(20);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenIdDoesNotExist()
    {
        var itemId = Guid.NewGuid();
        var query = new GetWarehouseItemQuery(itemId);

        _warehouseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseItem?)null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Warehouse item with ID '{itemId}' was not found.");
    }
}