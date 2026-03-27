using FluentAssertions;
using InventoryService.Application.AppExceptions;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Features.StoreIngredient.Commands.CheckEnoughStock;
using InventoryService.Application.Features.StoreIngredient.Commands.ReserveStock;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Services;
using MediatR;
using Moq;

namespace InventoryService.Application.Tests.Features.StoreIngredient;

public class ReserveStockHandlerTests
{
    private readonly Mock<IStoreIngredientRepository> _storeRepoMock;
    private readonly Mock<IRecipeRepository> _recipeRepoMock;
    private readonly Mock<IInventoryDomainService> _domainServiceMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ReserveStockHandler _handler;

    public ReserveStockHandlerTests()
    {
        _storeRepoMock = new Mock<IStoreIngredientRepository>();
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _domainServiceMock = new Mock<IInventoryDomainService>();
        _mediatorMock = new Mock<IMediator>();

        _handler = new ReserveStockHandler(
            _storeRepoMock.Object,
            _recipeRepoMock.Object,
            _domainServiceMock.Object,
            _mediatorMock.Object);
    }

    // 1. Phủ nhánh: Không có item nào để check (Dòng 41-44)
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldReturnEnough_WhenItemsIncomplete(bool isNull)
    {
        // Arrange
        var cmd = new ReserveStockCommand { Items = isNull ? null! : new List<AmountVariant>() };

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsEnough.Should().BeTrue();
        result.ShortageItems.Should().BeEmpty();
        _mediatorMock.Verify(x => x.Send(It.IsAny<CheckEnoughStockCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // 2. Phủ nhánh: Check không đủ tồn kho (Dòng 56-59)
    [Fact]
    public async Task Handle_ShouldReturnShortageImmediately_WhenCheckFails()
    {
        // Arrange
        var cmd = new ReserveStockCommand { Items = new List<AmountVariant> { new() { VariantId = Guid.NewGuid(), Amount = 10 } } };
        var checkRes = new CheckEnoughStockRes
        {
            IsEnough = false,
            ShortageItems = new List<AmountVariant> { new() { VariantId = cmd.Items[0].VariantId, Sku = "SKU01", Amount = 5 } }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CheckEnoughStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkRes);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsEnough.Should().BeFalse();
        result.ShortageItems.Should().NotBeNullOrEmpty();
        _recipeRepoMock.Verify(x => x.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // 3. Phủ nhánh: Đủ hàng nhưng DB không có công thức (Dòng 65-68)
    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenRecipesNotFound()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var cmd = new ReserveStockCommand { Items = new List<AmountVariant> { new() { VariantId = variantId } } };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CheckEnoughStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CheckEnoughStockRes { IsEnough = true });

        // Giả lập Repository trả về null hoặc rỗng
        _recipeRepoMock.Setup(r => r.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Recipe>());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<BusinessException>();
    }

    // 4. Happy Path: Đủ hàng -> Tính công thức -> Ghi Queue thành công (Dòng 70-96)
    [Fact]
    public async Task Handle_ShouldAddItemsToQueue_WhenStockIsReservedSuccessfully()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var cmd = new ReserveStockCommand
        {
            StoreId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Items = new List<AmountVariant> { new() { VariantId = variantId, Amount = 2 } }
        };

        // Mock 1: Mediator check đủ hàng
        _mediatorMock.Setup(m => m.Send(It.IsAny<CheckEnoughStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CheckEnoughStockRes { IsEnough = true, ShortageItems = new List<AmountVariant>() });

        // Mock 2: Repository tìm thấy công thức
        _recipeRepoMock.Setup(r => r.GetRecipesByVariantIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Recipe> { new() { VariantId = variantId, IngredientId = Guid.NewGuid() } });

        // Mock 3: Domain Service tính toán nguyên liệu
        var requiredIngredients = new Dictionary<Guid, decimal> { { Guid.NewGuid(), 5.5m } };
        _domainServiceMock.Setup(d => d.CalculateRequiredIngredients(It.IsAny<IEnumerable<(Guid, int)>>(), It.IsAny<List<Domain.Entities.Recipe>>()))
            .Returns(requiredIngredients);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsEnough.Should().BeTrue();

        // Kiểm tra Batch Insert được gọi đúng 1 lần với số lượng đúng
        _storeRepoMock.Verify(repo => repo.AddToQueueBatchAsync(
            It.Is<List<IngredientQueue>>(list => list.Count == requiredIngredients.Count && list[0].OrderId == cmd.OrderId)
        ), Times.Once);
    }
}