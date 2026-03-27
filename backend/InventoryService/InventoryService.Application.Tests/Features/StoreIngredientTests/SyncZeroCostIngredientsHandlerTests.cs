using FluentAssertions;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Features.StoreIngredient.Commands.RecalculateVariantCosts;
using InventoryService.Application.Features.StoreIngredient.Commands.SyncZeroCostIngredients;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryService.Application.Tests.Features.StoreIngredient;

public class SyncZeroCostIngredientsHandlerTests
{
    private readonly Mock<IIngredientRepository> _ingredientRepoMock;
    private readonly Mock<ISupplierServiceClient> _supplierClientMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<SyncZeroCostIngredientsHandler>> _loggerMock;
    private readonly SyncZeroCostIngredientsHandler _handler;

    public SyncZeroCostIngredientsHandlerTests()
    {
        _ingredientRepoMock = new Mock<IIngredientRepository>();
        _supplierClientMock = new Mock<ISupplierServiceClient>();
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<SyncZeroCostIngredientsHandler>>();

        _handler = new SyncZeroCostIngredientsHandler(
            _ingredientRepoMock.Object,
            _supplierClientMock.Object,
            _loggerMock.Object,
            _mediatorMock.Object);
    }

    [Fact] // 1. Phủ nhánh: Không có nguyên liệu (Dòng 31)
    public async Task Handle_ShouldReturnZero_WhenNoIngredientsFound()
    {
        _ingredientRepoMock.Setup(x => x.GetZeroCostIngredientsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingredient>());
        var result = await _handler.Handle(new SyncZeroCostIngredientsCommand(), CancellationToken.None);
        result.UpdatedCount.Should().Be(0);
    }

    [Fact] // 2. Phủ nhánh: gRPC lỗi + LogError (Dòng 42-51)
    public async Task Handle_ShouldLogError_WhenSupplierServiceFails()
    {
        _ingredientRepoMock.Setup(x => x.GetZeroCostIngredientsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingredient> { new() { Id = Guid.NewGuid() } });
        _supplierClientMock.Setup(x => x.GetLatestPriceForIngredientsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("gRPC Error"));

        await _handler.Handle(new SyncZeroCostIngredientsCommand(), CancellationToken.None);

        _loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }

    [Fact] // 3. Happy Path: Update thành công (Dòng 53-85)
    public async Task Handle_ShouldUpdate_WhenPricesFound()
    {
        var id = Guid.NewGuid();
        var ingredients = new List<Ingredient> { new() { Id = id, Cost = 0 } };
        var prices = new Dictionary<Guid, decimal> { { id, 10000m } };

        _ingredientRepoMock.Setup(x => x.GetZeroCostIngredientsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ingredients);
        _supplierClientMock.Setup(x => x.GetLatestPriceForIngredientsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(prices);

        var result = await _handler.Handle(new SyncZeroCostIngredientsCommand(), CancellationToken.None);

        result.UpdatedCount.Should().Be(1);
        _ingredientRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<RecalculateVariantCostsByIngredientCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact] // 4. Phủ nhánh Catch Mediator + LogError (Dòng 76-79)
    public async Task Handle_ShouldLogError_WhenMediatorFails()
    {
        var id = Guid.NewGuid();
        _ingredientRepoMock.Setup(x => x.GetZeroCostIngredientsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Ingredient> { new() { Id = id } });
        _supplierClientMock.Setup(x => x.GetLatestPriceForIngredientsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, decimal> { { id, 5000m } });
        _mediatorMock.Setup(m => m.Send(It.IsAny<RecalculateVariantCostsByIngredientCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Mediator Error"));

        await _handler.Handle(new SyncZeroCostIngredientsCommand(), CancellationToken.None);

        _loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.AtLeastOnce);
    }

    [Fact] // 5. Phủ nốt nhánh rẽ if (newCost > 0) là false
    public async Task Handle_ShouldSkip_WhenPriceIsZero()
    {
        var id = Guid.NewGuid();
        _ingredientRepoMock.Setup(x => x.GetZeroCostIngredientsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Ingredient> { new() { Id = id } });
        _supplierClientMock.Setup(x => x.GetLatestPriceForIngredientsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, decimal> { { id, 0m } });

        var result = await _handler.Handle(new SyncZeroCostIngredientsCommand(), CancellationToken.None);

        result.UpdatedCount.Should().Be(0);
        _mediatorMock.Verify(m => m.Send(It.IsAny<RecalculateVariantCostsByIngredientCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}