using FluentAssertions;
using InventoryService.Application.Constants;
using InventoryService.Application.Features.Ingrdient.Commands.UpdateIngredient;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using Moq;
using Xunit;
using MediatR;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InventoryService.Application.Dtos.Ingredient;

namespace InventoryService.Application.Tests.Features.IngredientTests;

public class UpdateIngredientHandlerTests
{
    private readonly Mock<IIngredientRepository> _ingredientRepoMock;
    private readonly Mock<IIngredientCategoryRepository> _categoryRepoMock;
    private readonly UpdateIngredientHandler _handler;

    public UpdateIngredientHandlerTests()
    {
        _ingredientRepoMock = new Mock<IIngredientRepository>();
        _categoryRepoMock = new Mock<IIngredientCategoryRepository>();
        _handler = new UpdateIngredientHandler(_ingredientRepoMock.Object, _categoryRepoMock.Object);
    }

    // 1. Phủ nhánh: CategoryId tồn tại nhưng DB không có (Throw Exception)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenCategoryNotFound()
    {
        var categoryId = Guid.NewGuid();
        var request = new UpdateIngredientReq { CategoryId = categoryId };
        var cmd = new UpdateIngredientCommand(request);

        _categoryRepoMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync((IngredientCategory)null!);

        await _handler.Invoking(x => x.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<DirectoryNotFoundException>();
    }

    // 2. Phủ nhánh: Ingredient Id không tồn tại (Throw Exception)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenIngredientNotFound()
    {
        var request = new UpdateIngredientReq { Id = Guid.NewGuid(), CategoryId = null };
        var cmd = new UpdateIngredientCommand(request);

        _ingredientRepoMock.Setup(x => x.GetByIdAsync(request.Id)).ReturnsAsync((Ingredient)null!);

        await _handler.Invoking(x => x.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<DirectoryNotFoundException>();
    }

    // 3. Phủ XANH 100% Line & Branch: Cập nhật đầy đủ các trường hợp hợp lệ
    [Fact]
    public async Task Handle_ShouldUpdateAllFields_ToCoverPositiveBranches()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var request = new UpdateIngredientReq
        {
            Id = ingredientId,
            Name = "New Name",
            CategoryId = categoryId,
            // SỬA Ở ĐÂY: Dùng hằng số chuẩn để pass CheckValidBaseUnit.IsValid
            BaseUnit = BaseUnitConstants.Gram
        };
        var cmd = new UpdateIngredientCommand(request);

        var itemInDb = new Ingredient { Id = ingredientId, Name = "Old Name", BaseUnit = "OldUnit" };

        _categoryRepoMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(new IngredientCategory());
        _ingredientRepoMock.Setup(x => x.GetByIdAsync(ingredientId)).ReturnsAsync(itemInDb);

        // Act
        await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        itemInDb.Name.Should().Be("New Name");
        itemInDb.CategoryId.Should().Be(categoryId);
        itemInDb.BaseUnit.Should().Be(BaseUnitConstants.Gram);

        _ingredientRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Ingredient>()), Times.Once);
    }

    // 4. Phủ BRANCH 100%: Khi các trường trong Request bị NULL hoặc TRỐNG
    [Fact]
    public async Task Handle_ShouldNotUpdate_WhenFieldsAreNullOrInvalid()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var request = new UpdateIngredientReq
        {
            Id = ingredientId,
            Name = null,        // Đi vào nhánh false của if Name != null
            CategoryId = null,  // Đi vào nhánh false của if CategoryId != null
            BaseUnit = ""       // Đi vào nhánh false của IsNullOrEmpty hoặc IsValid
        };
        var cmd = new UpdateIngredientCommand(request);

        var itemInDb = new Ingredient { Id = ingredientId, Name = "Original", BaseUnit = "Piece", CategoryId = Guid.NewGuid() };

        _ingredientRepoMock.Setup(x => x.GetByIdAsync(ingredientId)).ReturnsAsync(itemInDb);

        // Act
        await _handler.Handle(cmd, CancellationToken.None);

        // Assert: Các giá trị cũ phải được giữ nguyên
        itemInDb.Name.Should().Be("Original");
        itemInDb.BaseUnit.Should().Be("Piece");

        _ingredientRepoMock.Verify(x => x.UpdateAsync(itemInDb), Times.Once);
    }
}