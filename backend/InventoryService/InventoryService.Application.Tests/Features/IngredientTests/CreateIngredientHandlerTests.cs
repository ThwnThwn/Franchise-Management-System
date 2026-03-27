using FluentAssertions;
using InventoryService.Application.Features.Ingrdient.Commands.CreateIngredient;
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

public class CreateIngredientHandlerTests
{
    private readonly Mock<IIngredientRepository> _ingredientRepoMock;
    private readonly Mock<IIngredientCategoryRepository> _categoryRepoMock;
    private readonly CreateIngredientHandler _handler;

    public CreateIngredientHandlerTests()
    {
        _ingredientRepoMock = new Mock<IIngredientRepository>();
        _categoryRepoMock = new Mock<IIngredientCategoryRepository>();
        _handler = new CreateIngredientHandler(_ingredientRepoMock.Object, _categoryRepoMock.Object);
    }

    // 1. Phủ nhánh: CategoryId có giá trị và tồn tại (Happy Path)
    [Fact]
    public async Task Handle_ShouldCreateSuccessfully_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new CreateIngredientReq
        {
            Name = "Bột mì",
            BaseUnit = "Kg",
            CategoryId = categoryId
        };
        var cmd = new CreateIngredientCommand(request);

        _categoryRepoMock.Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(new IngredientCategory()); // Giả lập category tồn tại

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _ingredientRepoMock.Verify(x => x.AddAsync(It.Is<InventoryService.Domain.Entities.Ingredient>(
            i => i.Name == request.Name && i.CategoryId == categoryId)), Times.Once);
    }

    // 2. Phủ nhánh: CategoryId có giá trị nhưng KHÔNG tồn tại (Dòng 34-36)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new CreateIngredientReq { CategoryId = categoryId };
        var cmd = new CreateIngredientCommand(request);

        _categoryRepoMock.Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync((IngredientCategory)null!); // Giả lập category không tồn tại

        // Act & Assert
        await _handler.Invoking(x => x.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<DirectoryNotFoundException>();

        _ingredientRepoMock.Verify(x => x.AddAsync(It.IsAny<InventoryService.Domain.Entities.Ingredient>()), Times.Never);
    }

    // 3. Phủ nhánh: CategoryId là NULL (Dòng 32)
    [Fact]
    public async Task Handle_ShouldCreateSuccessfully_WhenCategoryIdIsNull()
    {
        // Arrange
        var request = new CreateIngredientReq { Name = "Muối", BaseUnit = "G", CategoryId = null };
        var cmd = new CreateIngredientCommand(request);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _categoryRepoMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _ingredientRepoMock.Verify(x => x.AddAsync(It.Is<InventoryService.Domain.Entities.Ingredient>(
            i => i.Name == request.Name && i.CategoryId == null)), Times.Once);
    }
}