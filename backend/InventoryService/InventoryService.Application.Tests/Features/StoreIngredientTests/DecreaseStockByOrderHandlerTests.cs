using FluentAssertions;
using InventoryService.Application.Abstractions;
using InventoryService.Application.AppExceptions;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Features.StoreIngredient.Commands.DecreaseStockByOrder;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Application.Signals;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryService.Application.Tests.Features.StoreIngredient;

public class DecreaseStockByOrderCommandHandlerTests
{
    private readonly Mock<IStoreIngredientRepository> _storeRepoMock;
    private readonly Mock<IRecipeRepository> _recipeRepoMock;
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
    private readonly Mock<IOutboxSignal> _outboxSignalMock;
    private readonly Mock<IInventoryDbContext> _dbContextMock;
    private readonly Mock<ILogger<DecreaseStockByOrderCommandHandler>> _loggerMock;
    private readonly Mock<IInventoryDomainService> _domainServiceMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;
    private readonly DecreaseStockByOrderCommandHandler _handler;

    public DecreaseStockByOrderCommandHandlerTests()
    {
        _storeRepoMock = new Mock<IStoreIngredientRepository>();
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _outboxRepositoryMock = new Mock<IOutboxRepository>();
        _outboxSignalMock = new Mock<IOutboxSignal>();
        _dbContextMock = new Mock<IInventoryDbContext>();
        _loggerMock = new Mock<ILogger<DecreaseStockByOrderCommandHandler>>();
        _domainServiceMock = new Mock<IInventoryDomainService>();
        _transactionMock = new Mock<IDbContextTransaction>();

        _dbContextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _handler = new DecreaseStockByOrderCommandHandler(
            _storeRepoMock.Object, _recipeRepoMock.Object, _outboxRepositoryMock.Object,
            _outboxSignalMock.Object, _loggerMock.Object, _dbContextMock.Object, _domainServiceMock.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldReturnUnit_WhenNoItems(bool isNull)
    {
        var req = new DecreaseStockByOrderReq { Items = isNull ? null! : new List<AmountVariant>() };
        var res = await _handler.Handle(new DecreaseStockByOrderCommand(req), CancellationToken.None);
        res.Should().Be(Unit.Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldThrowException_WhenNoRecipesFound(bool isNull)
    {
        var req = new DecreaseStockByOrderReq { Items = new List<AmountVariant> { new() { VariantId = Guid.NewGuid() } } };
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(isNull ? (List<Domain.Entities.Recipe>?)null : new List<Domain.Entities.Recipe>());

        await _handler.Invoking(x => x.Handle(new DecreaseStockByOrderCommand(req), CancellationToken.None)).Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenStoreIngredientsCountMismatch()
    {
        var id = Guid.NewGuid();
        var req = new DecreaseStockByOrderReq { Items = new List<AmountVariant> { new() { VariantId = id } } };
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Domain.Entities.Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Domain.Entities.Recipe>>())).Returns(new Dictionary<Guid, decimal> { { id, 10 } });
        _storeRepoMock.Setup(x => x.GetStockByStoreIdAndIngredientIdsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<InventoryService.Domain.Entities.StoreIngredient>());

        await _handler.Invoking(x => x.Handle(new DecreaseStockByOrderCommand(req), CancellationToken.None)).Should().ThrowAsync<BusinessException>();
    }

    [Theory]
    [InlineData(false, 10, true)] // Case: Ingredient null
    [InlineData(true, null, true)]  // Case: Threshold null
    [InlineData(true, 5, false)]   // Case: IsLowStock false
    public async Task Handle_ShouldCoverAllDeepLogicAndPrivateMethod(bool hasIngredient, int? threshold, bool isLow)
    {
        var id = Guid.NewGuid();
        var req = new DecreaseStockByOrderReq { StoreId = Guid.NewGuid(), Items = new List<AmountVariant> { new() { VariantId = id, Amount = 1 } } };
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Domain.Entities.Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny< List<Domain.Entities.Recipe >>())).Returns(new Dictionary<Guid, decimal> { { id, 10 } });

        var stock = new InventoryService.Domain.Entities.StoreIngredient { IngredientId = id };
        if (hasIngredient) stock.Ingredient = new Ingredient { Name = "Arabica" };
        _storeRepoMock.Setup(x => x.GetStockByStoreIdAndIngredientIdsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<InventoryService.Domain.Entities.StoreIngredient> { stock });

        if (!hasIngredient && threshold == 10) // Case Result NULL
        {
            _storeRepoMock.Setup(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), id, It.IsAny<decimal>())).ReturnsAsync((DecreaseStockResult?)null);
            await _handler.Invoking(x => x.Handle(new DecreaseStockByOrderCommand(req), CancellationToken.None)).Should().ThrowAsync<BusinessException>();
        }
        else
        {
            var resDto = new DecreaseStockResult { Quantity = 1, MinThreshold = threshold };
            _storeRepoMock.Setup(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), id, It.IsAny<decimal>())).ReturnsAsync(resDto);
            _domainServiceMock.Setup(x => x.IsLowStock(It.IsAny<decimal>(), It.IsAny<decimal>())).Returns(isLow);

            await _handler.Handle(new DecreaseStockByOrderCommand(req), CancellationToken.None);

            if (isLow) _outboxRepositoryMock.Verify(x => x.Add(It.IsAny<OutboxMessage>()), Times.AtLeastOnce);
        }
    }

    [Fact]
    public async Task Handle_ShouldRollback_OnSystemCrash()
    {
        var id = Guid.NewGuid();
        var req = new DecreaseStockByOrderReq { Items = new List<AmountVariant> { new() { VariantId = id } } };
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Domain.Entities.Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Domain.Entities.Recipe>>())).Returns(new Dictionary<Guid, decimal> { { id, 10 } });
        _storeRepoMock.Setup(x => x.GetStockByStoreIdAndIngredientIdsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<InventoryService.Domain.Entities.StoreIngredient> { new() { IngredientId = id } });
        _storeRepoMock.Setup(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>())).ThrowsAsync(new Exception("Crash"));

        await _handler.Invoking(x => x.Handle(new DecreaseStockByOrderCommand(req), CancellationToken.None)).Should().ThrowAsync<Exception>();
        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}