using Xunit;
using Moq;
using FluentAssertions;
using InventoryService.Application.Features.StoreIngredient.Commands.DecreaseStockByOrder;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Application.Signals;
using InventoryService.Application.Abstractions;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Domain.Services;
using InventoryService.Domain.Entities;
using InventoryService.Application.AppExceptions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;
using MediatR;

namespace InventoryService.Application.Tests.Features.StoreIngredient;

public class DecreaseStockByOrderCommandHandlerTests
{
    private readonly Mock<IStoreIngredientRepository> _storeRepoMock;
    private readonly Mock<IRecipeRepository> _recipeRepoMock;
    private readonly Mock<IOutboxRepository> _outboxRepoMock;
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
        _outboxRepoMock = new Mock<IOutboxRepository>();
        _outboxSignalMock = new Mock<IOutboxSignal>();
        _dbContextMock = new Mock<IInventoryDbContext>();
        _loggerMock = new Mock<ILogger<DecreaseStockByOrderCommandHandler>>();
        _domainServiceMock = new Mock<IInventoryDomainService>();
        _transactionMock = new Mock<IDbContextTransaction>();

        // Setup Transaction thông qua Interface (né lỗi DatabaseFacade)
        _dbContextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _handler = new DecreaseStockByOrderCommandHandler(
            _storeRepoMock.Object, _recipeRepoMock.Object, _outboxRepoMock.Object,
            _outboxSignalMock.Object, _loggerMock.Object, _dbContextMock.Object, _domainServiceMock.Object);
    }

    // 1. Phủ nhánh: Items rỗng (Dòng 52)
    [Fact]
    public async Task Handle_ShouldReturnUnit_WhenItemsAreEmpty()
    {
        var command = new DecreaseStockByOrderCommand(new DecreaseStockByOrderReq { Items = new List<AmountVariant>() });
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().Be(Unit.Value);
    }

    // 2. Phủ nhánh: recipes == null (Dòng 60)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenNoRecipesFound()
    {
        var command = new DecreaseStockByOrderCommand(new DecreaseStockByOrderReq
        {
            Items = new List<AmountVariant> { new() { VariantId = Guid.NewGuid() } }
        });
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Recipe>?)null);

        await _handler.Invoking(x => x.Handle(command, CancellationToken.None)).Should().ThrowAsync<BusinessException>();
    }

    // 3. Phủ nhánh: Cửa hàng thiếu mã nguyên liệu (Dòng 76)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenStoreIngredientsCountMismatch()
    {
        var variantId = Guid.NewGuid();
        var command = new DecreaseStockByOrderCommand(new DecreaseStockByOrderReq
        {
            StoreId = Guid.NewGuid(),
            Items = new List<AmountVariant> { new() { VariantId = variantId, Amount = 1 } }
        });

        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Recipe> { new() { VariantId = variantId, IngredientId = Guid.NewGuid() } });

        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Recipe>>()))
            .Returns(new Dictionary<Guid, decimal> { { Guid.NewGuid(), 10 } });

        _storeRepoMock.Setup(x => x.GetStockByStoreIdAndIngredientIdsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.StoreIngredient>()); // Trả về list rỗng để lệch count

        await _handler.Invoking(x => x.Handle(command, CancellationToken.None)).Should().ThrowAsync<BusinessException>();
    }

    // 4. Phủ nhánh: Trừ kho thất bại (Dòng 92) + Rollback
    [Fact]
    public async Task Handle_ShouldThrowException_AndRollback_WhenDecreaseReturnsNull()
    {
        var ingredientId = Guid.NewGuid();
        var command = new DecreaseStockByOrderCommand(new DecreaseStockByOrderReq
        {
            StoreId = Guid.NewGuid(),
            Items = new List<AmountVariant> { new() { VariantId = Guid.NewGuid(), Amount = 1 } }
        });

        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Recipe>>()))
            .Returns(new Dictionary<Guid, decimal> { { ingredientId, 10 } });
        _storeRepoMock.Setup(x => x.GetStockByStoreIdAndIngredientIdsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.StoreIngredient> { new() { IngredientId = ingredientId } });

        _storeRepoMock.Setup(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync((DecreaseStockResult?)null);

        await _handler.Invoking(x => x.Handle(command, CancellationToken.None)).Should().ThrowAsync<BusinessException>();
        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // 5. Phủ nhánh: Thành công + Low Stock + MinThreshold NULL (Phủ dấu ?? 0 ở dòng 102 & 117)
    [Fact]
    public async Task Handle_ShouldPulse_WhenLowStockDetected_WithNullThreshold()
    {
        var ingredientId = Guid.NewGuid();
        var command = new DecreaseStockByOrderCommand(new DecreaseStockByOrderReq { StoreId = Guid.NewGuid(), Items = new List<AmountVariant> { new() { VariantId = Guid.NewGuid(), Amount = 1 } } });

        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Recipe>>())).Returns(new Dictionary<Guid, decimal> { { ingredientId, 10 } });
        _storeRepoMock.Setup(x => x.GetStockByStoreIdAndIngredientIdsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.StoreIngredient> { new() { IngredientId = ingredientId, Ingredient = new Ingredient { Name = "Bot" } } });

        _storeRepoMock.Setup(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync(new DecreaseStockResult { Quantity = 2, MinThreshold = null }); // Threshold NULL

        _domainServiceMock.Setup(x => x.IsLowStock(2, 0)).Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        _outboxRepoMock.Verify(x => x.Add(It.IsAny<OutboxMessage>()), Times.Once);
        _outboxSignalMock.Verify(x => x.Pulse(), Times.Once);
        _transactionMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // 6. Phủ nhánh: Thành công nhưng KHÔNG bị Low Stock (Dòng 102 FALSE)
    [Fact]
    public async Task Handle_ShouldNotAddOutbox_WhenStockIsFine()
    {
        var ingredientId = Guid.NewGuid();
        var command = new DecreaseStockByOrderCommand(new DecreaseStockByOrderReq { StoreId = Guid.NewGuid(), Items = new List<AmountVariant> { new() } });

        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Recipe>>())).Returns(new Dictionary<Guid, decimal> { { ingredientId, 10 } });
        _storeRepoMock.Setup(x => x.GetStockByStoreIdAndIngredientIdsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.StoreIngredient> { new() { IngredientId = ingredientId } });

        _storeRepoMock.Setup(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync(new DecreaseStockResult { Quantity = 100, MinThreshold = 5 });

        _domainServiceMock.Setup(x => x.IsLowStock(100, 5)).Returns(false);

        await _handler.Handle(command, CancellationToken.None);

        _outboxRepoMock.Verify(x => x.Add(It.IsAny<OutboxMessage>()), Times.Never);
        _outboxSignalMock.Verify(x => x.Pulse(), Times.Once);
    }

    // 7. Phủ nhánh: Lỗi hệ thống bất ngờ (Dòng 130 Catch Exception)
    [Fact]
    public async Task Handle_ShouldRollback_OnSystemError()
    {
        var command = new DecreaseStockByOrderCommand(new DecreaseStockByOrderReq { Items = new List<AmountVariant> { new() } });
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Recipe>>())).Returns(new Dictionary<Guid, decimal> { { Guid.NewGuid(), 10 } });
        _storeRepoMock.Setup(x => x.GetStockByStoreIdAndIngredientIdsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Domain.Entities.StoreIngredient> { new() { IngredientId = Guid.NewGuid() } });

        // Lỗi xảy ra khi bắt đầu transaction
        _dbContextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Fatal"));

        await _handler.Invoking(x => x.Handle(command, CancellationToken.None)).Should().ThrowAsync<Exception>();
    }
}