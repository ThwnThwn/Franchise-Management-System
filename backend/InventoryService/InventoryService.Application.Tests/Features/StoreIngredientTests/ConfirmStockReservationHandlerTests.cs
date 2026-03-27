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

    // 1. Phủ Early Return - Check null hoặc Count == 0 (Dòng 46)
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldReturnTrue_WhenNoItemsToProcess(bool isNull)
    {
        var cmd = new ConfirmStockReservationCommand(Guid.NewGuid(), Guid.NewGuid(), isNull ? null! : new List<OrderPaidItem>());
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.Should().BeTrue();
    }

    // 2. Phủ InvalidOperationException - Dòng 61-64 (Đồng bộ với Handler mới)
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldThrowInvalidOperation_WhenRecipesProblem(bool isNull)
    {
        var cmd = new ConfirmStockReservationCommand(Guid.NewGuid(), Guid.NewGuid(), new List<OrderPaidItem> { new() });
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(isNull ? (List<Domain.Entities.Recipe>?)null : new List<Domain.Entities.Recipe>());

        await _handler.Invoking(x => x.Handle(cmd, CancellationToken.None)).Should().ThrowAsync<InvalidOperationException>();
        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // 3. Phủ toàn bộ logic rẽ nhánh trong vòng lặp (MinThreshold & LowStock)
    [Theory]
    [InlineData(null, true)]
    [InlineData(10, true)]
    [InlineData(10, false)]
    public async Task Handle_ShouldCoverAllLoopBranches(int? thresholdValue, bool isLow)
    {
        var id = Guid.NewGuid();
        var cmd = new ConfirmStockReservationCommand(Guid.NewGuid(), Guid.NewGuid(), new List<OrderPaidItem> { new() });
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Domain.Entities.Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Domain.Entities.Recipe>>())).Returns(new Dictionary<Guid, decimal> { { id, 1 } });

        var resDto = new DecreaseStockResult { Quantity = 1, MinThreshold = thresholdValue };
        _storeRepoMock.Setup(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), id, It.IsAny<decimal>())).ReturnsAsync(resDto);
        _domainServiceMock.Setup(x => x.IsLowStock(1, It.IsAny<decimal>())).Returns(isLow);

        await _handler.Handle(cmd, CancellationToken.None);

        if (isLow) _outboxSignalMock.Verify(x => x.Pulse(), Times.Once);
        else _outboxSignalMock.Verify(x => x.Pulse(), Times.Never);
    }

    // 4. Phủ BusinessException - Dòng 79
    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenDeductReturnsNull()
    {
        var id = Guid.NewGuid();
        var cmd = new ConfirmStockReservationCommand(Guid.NewGuid(), Guid.NewGuid(), new List<OrderPaidItem> { new() });
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Domain.Entities.Recipe> { new() });
        _domainServiceMock.Setup(x => x.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Domain.Entities.Recipe>>())).Returns(new Dictionary<Guid, decimal> { { id, 1 } });
        _storeRepoMock.Setup(x => x.DecreaseStockAtomicallyAsync(It.IsAny<Guid>(), id, It.IsAny<decimal>())).ReturnsAsync((DecreaseStockResult?)null);

        await _handler.Invoking(x => x.Handle(cmd, CancellationToken.None)).Should().ThrowAsync<BusinessException>();
    }

    // 5. Phủ Catch & Rollback
    [Fact]
    public async Task Handle_ShouldRollback_OnSystemError()
    {
        var cmd = new ConfirmStockReservationCommand(Guid.NewGuid(), Guid.NewGuid(), new List<OrderPaidItem> { new() });
        _storeRepoMock.Setup(x => x.RemoveQueueByOrderIdAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("Crash"));

        await _handler.Invoking(x => x.Handle(cmd, CancellationToken.None)).Should().ThrowAsync<Exception>();
        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}