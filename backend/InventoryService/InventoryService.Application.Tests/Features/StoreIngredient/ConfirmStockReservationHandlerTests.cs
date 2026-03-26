using FluentAssertions;
using InventoryService.Application.Abstractions;
using InventoryService.Application.AppExceptions;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Features.StoreIngredient.Commands.ConfirmStockReservation;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Application.Signals;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Events;
using Xunit;

namespace InventoryService.Application.Tests.Features.StoreIngredient;

public class ConfirmStockReservationHandlerTests
{
    private readonly Mock<IStoreIngredientRepository> _storeRepoMock;
    private readonly Mock<IRecipeRepository> _recipeRepoMock;
    private readonly Mock<IOutboxRepository> _outboxRepoMock;
    private readonly Mock<IOutboxSignal> _outboxSignalMock;
    private readonly Mock<IInventoryDbContext> _dbContextMock;
    private readonly Mock<ILogger<ConfirmStockReservationHandler>> _loggerMock;
    private readonly Mock<IInventoryDomainService> _domainServiceMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;
    private readonly ConfirmStockReservationHandler _handler;

    public ConfirmStockReservationHandlerTests()
    {
        _storeRepoMock = new Mock<IStoreIngredientRepository>();
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _outboxRepoMock = new Mock<IOutboxRepository>();
        _outboxSignalMock = new Mock<IOutboxSignal>();
        _dbContextMock = new Mock<IInventoryDbContext>();
        _loggerMock = new Mock<ILogger<ConfirmStockReservationHandler>>();
        _domainServiceMock = new Mock<IInventoryDomainService>();
        _transactionMock = new Mock<IDbContextTransaction>();

        _dbContextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _handler = new ConfirmStockReservationHandler(
            _storeRepoMock.Object, _recipeRepoMock.Object, _outboxRepoMock.Object,
            _outboxSignalMock.Object, _dbContextMock.Object, _loggerMock.Object, _domainServiceMock.Object);
    }

    // --- CASE 1: Phủ nhánh recipes == null (Dòng 58) ---
    [Fact]
    public async Task Handle_ShouldThrowException_WhenRecipesAreNull()
    {
        var command = new ConfirmStockReservationCommand(Guid.NewGuid(), Guid.NewGuid(), new List<OrderPaidItem>());
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync((List<Recipe>?)null);

        await _handler.Invoking(x => x.Handle(command, CancellationToken.None)).Should().ThrowAsync<Exception>();
        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- CASE 2: Phủ nhánh foreach KHÔNG chạy (Dòng 70 - Dictionary rỗng) ---
    [Fact]
    public async Task Handle_ShouldSucceed_WhenNoIngredientsToDeduct()
    {
        var command = new ConfirmStockReservationCommand(Guid.NewGuid(), Guid.NewGuid(), new List<OrderPaidItem> { new() });
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Recipe> { new() });
        // Trả về Dictionary rỗng để skip foreach
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Recipe>>()))
            .Returns(new Dictionary<Guid, decimal>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _storeRepoMock.Verify(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
    }

    // --- CASE 3: Phủ nhánh result == null (Dòng 82 - Exception) ---
    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenDeductReturnsNull()
    {
        var ingredientId = Guid.NewGuid();
        var command = new ConfirmStockReservationCommand(Guid.NewGuid(), Guid.NewGuid(), new List<OrderPaidItem> { new() });
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Recipe>>()))
            .Returns(new Dictionary<Guid, decimal> { { ingredientId, 10 } });
        _storeRepoMock.Setup(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>())).ReturnsAsync((DecreaseStockResult?)null);

        await _handler.Invoking(x => x.Handle(command, CancellationToken.None)).Should().ThrowAsync<BusinessException>();
        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- CASE 4: Phủ nhánh MinThreshold == NULL (Dòng 90 & 104 - Phủ dấu ?? 0) ---
    [Fact]
    public async Task Handle_ShouldCoverNullThresholdConditions()
    {
        var ingredientId = Guid.NewGuid();
        var command = new ConfirmStockReservationCommand(Guid.NewGuid(), Guid.NewGuid(), new List<OrderPaidItem> { new() });
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Recipe>>()))
            .Returns(new Dictionary<Guid, decimal> { { ingredientId, 10 } });

        // MinThreshold = null để phủ condition ?? 0
        var resultDto = new DecreaseStockResult { Quantity = 1, MinThreshold = null };
        _storeRepoMock.Setup(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>())).ReturnsAsync(resultDto);
        _domainServiceMock.Setup(x => x.IsLowStock(1, 0)).Returns(true); // Verify số 0 được dùng

        await _handler.Handle(command, CancellationToken.None);

        _domainServiceMock.Verify(x => x.IsLowStock(1, 0), Times.Once);
        _outboxSignalMock.Verify(x => x.Pulse(), Times.Once);
    }

    // --- CASE 5: Phủ nhánh MinThreshold có giá trị (Dòng 90 - Phủ vế trái của ??) ---
    [Fact]
    public async Task Handle_ShouldCoverValueThresholdConditions()
    {
        var ingredientId = Guid.NewGuid();
        var command = new ConfirmStockReservationCommand(Guid.NewGuid(), Guid.NewGuid(), new List<OrderPaidItem> { new() });
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Recipe>>())).Returns(new Dictionary<Guid, decimal> { { ingredientId, 10 } });

        var resultDto = new DecreaseStockResult { Quantity = 5, MinThreshold = 10 };
        _storeRepoMock.Setup(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>())).ReturnsAsync(resultDto);
        _domainServiceMock.Setup(x => x.IsLowStock(5, 10)).Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        _domainServiceMock.Verify(x => x.IsLowStock(5, 10), Times.Once);
    }

    // --- CASE 6: Phủ nhánh lowStockDetected == FALSE (Dòng 115) ---
    [Fact]
    public async Task Handle_ShouldNotPulse_WhenNoLowStockDetected()
    {
        var ingredientId = Guid.NewGuid();
        var command = new ConfirmStockReservationCommand(Guid.NewGuid(), Guid.NewGuid(), new List<OrderPaidItem> { new() });
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Recipe>>())).Returns(new Dictionary<Guid, decimal> { { ingredientId, 10 } });
        _storeRepoMock.Setup(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>())).ReturnsAsync(new DecreaseStockResult { Quantity = 100, MinThreshold = 10 });
        _domainServiceMock.Setup(x => x.IsLowStock(100, 10)).Returns(false);

        await _handler.Handle(command, CancellationToken.None);

        _outboxSignalMock.Verify(x => x.Pulse(), Times.Never);
    }

    // --- CASE 7: Phủ nhánh Transaction lỗi lúc khởi tạo (Dòng 47) ---
    [Fact]
    public async Task Handle_ShouldRollback_WhenTransactionCreationFails()
    {
        _dbContextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Conn Error"));

        await _handler.Invoking(x => x.Handle(new ConfirmStockReservationCommand(Guid.NewGuid(), Guid.NewGuid(), null!), CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}