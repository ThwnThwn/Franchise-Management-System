using FluentAssertions;
using InventoryService.Application.AppExceptions;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Features.StoreIngredient.Commands.CaculateCostIngredientInStock;
using InventoryService.Application.Features.StoreIngredient.Commands.IncreaseStock;
using InventoryService.Application.Features.StoreIngredient.Commands.RecalculateVariantCosts;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryService.Application.Tests.Features.StoreIngredient;

public class IncreaseStockCommandHandlerTests
{
    private readonly Mock<IStoreIngredientRepository> _storeRepoMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<IncreaseStockCommandHandler>> _loggerMock;
    private readonly IncreaseStockCommandHandler _handler;

    public IncreaseStockCommandHandlerTests()
    {
        _storeRepoMock = new Mock<IStoreIngredientRepository>();
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<IncreaseStockCommandHandler>>();

        _handler = new IncreaseStockCommandHandler(
            _storeRepoMock.Object,
            _mediatorMock.Object,
            _loggerMock.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldThrowBusinessException_WhenItemsIncomplete(bool isNull)
    {
        var req = new IncreaseStockReq { Items = isNull ? null! : new List<IngredientStockItem>() };
        var command = new IncreaseStockCommand(req);

        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenEverythingIsOk()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var req = new IncreaseStockReq
        {
            StoreId = Guid.NewGuid(),
            Items = new List<IngredientStockItem>
            {
                new() { IngredientId = ingredientId, Quantity = 10, UnitPrice = 5000 }
            }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CaculateCostIngredientInStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CaculateCostIngredientInStockResult { IngredientName = "Cafe", OldCost = 4000, NewCost = 4500 });

        // FIX: Đổi tên Result theo đúng định nghĩa của Duy
        _mediatorMock.Setup(m => m.Send(It.IsAny<RecalculateVariantCostsByIngredientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RecalculateVariantCostsResult { Message = "Success" });

        // Act
        var result = await _handler.Handle(new IncreaseStockCommand(req), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _storeRepoMock.Verify(x => x.IncreaseStockAsync(req.StoreId, req.Items), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldContinue_WhenMacCalculationFails()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var req = new IncreaseStockReq
        {
            Items = new List<IngredientStockItem>
            {
                new() { IngredientId = id1 },
                new() { IngredientId = id2 }
            }
        };

        _mediatorMock.Setup(m => m.Send(It.Is<CaculateCostIngredientInStockCommand>(c => c.IngredientId == id1), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("MAC Error"));

        _mediatorMock.Setup(m => m.Send(It.Is<CaculateCostIngredientInStockCommand>(c => c.IngredientId == id2), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CaculateCostIngredientInStockResult { IngredientName = "Milk" });

        // FIX: Đổi tên Result
        _mediatorMock.Setup(m => m.Send(It.IsAny<RecalculateVariantCostsByIngredientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RecalculateVariantCostsResult { Message = "Ok" });

        // Act
        await _handler.Handle(new IncreaseStockCommand(req), CancellationToken.None);

        // Assert
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error calculating MAC for ingredient {id1}")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldContinue_WhenVariantRecalculationFails()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var req = new IncreaseStockReq { Items = new List<IngredientStockItem> { new() { IngredientId = ingredientId } } };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CaculateCostIngredientInStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CaculateCostIngredientInStockResult { IngredientName = "Sugar" });

        _mediatorMock.Setup(m => m.Send(It.IsAny<RecalculateVariantCostsByIngredientCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Variant DB Error"));

        // Act
        await _handler.Handle(new IncreaseStockCommand(req), CancellationToken.None);

        // Assert
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error recalculating variant costs for ingredient {ingredientId}")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}