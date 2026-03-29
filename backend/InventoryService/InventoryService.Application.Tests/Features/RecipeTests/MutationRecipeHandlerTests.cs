using FluentAssertions;
using InventoryService.Application.Abstractions;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Dtos.Product;
using InventoryService.Application.Dtos.Recipe;
using InventoryService.Application.Features.Recipe.Commands.MutationRecipe;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace InventoryService.Application.Tests.Features.Recipe;

public class MutationRecipeHandlerTests
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock;
    private readonly Mock<IProductServiceClient> _productClientMock;
    private readonly Mock<IInventoryDbContext> _dbContextMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;
    private readonly MutationRecipeHandler _handler;

    public MutationRecipeHandlerTests()
    {
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _productClientMock = new Mock<IProductServiceClient>();
        _dbContextMock = new Mock<IInventoryDbContext>();
        _transactionMock = new Mock<IDbContextTransaction>();

        // SETUP: Mock trực tiếp hàm BeginTransactionAsync của Interface (Yêu cầu Handler gọi trực tiếp qua DbContext)
        _dbContextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _handler = new MutationRecipeHandler(
            _recipeRepoMock.Object,
            _productClientMock.Object,
            _dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenVariantNotFound()
    {
        // Arrange
        var request = new MutationRecipeReqDto { VariantId = Guid.NewGuid() };
        _productClientMock.Setup(x => x.GetVariantById(request.VariantId)).ReturnsAsync((ProductVariantRespone)null!);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new MutationRecipeCommand(request), CancellationToken.None))
            .Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldSyncRecipesCorrectly_WhenValidRequest()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var ingAddId = Guid.NewGuid();      // Phủ nhánh 1: Xử lý thêm
        var ingUpdateId = Guid.NewGuid();   // Phủ nhánh 2: Xử lý sửa
        var ingRemoveId = Guid.NewGuid();   // Phủ nhánh 3: Xử lý xóa

        var request = new MutationRecipeReqDto
        {
            VariantId = variantId,
            Ingredients = new List<RecipeIngredientRequest>
            {
                new() { IngredientId = ingAddId, Quantity = 10 },
                new() { IngredientId = ingUpdateId, Quantity = 20 }
            }
        };

        var existingRecipes = new List<InventoryService.Domain.Entities.Recipe>
        {
            new() { IngredientId = ingUpdateId, Quantity = 5, Id = Guid.NewGuid() },
            new() { IngredientId = ingRemoveId, Id = Guid.NewGuid() }
        };

        _productClientMock.Setup(x => x.GetVariantById(variantId)).ReturnsAsync(new ProductVariantRespone());
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdAsync(variantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRecipes);

        // Act
        await _handler.Handle(new MutationRecipeCommand(request), CancellationToken.None);

        // Assert
        // Tách giá trị ra trước để tránh lỗi NotSupportedException trong Moq Verify
        var removedRecipeId = existingRecipes.First(r => r.IngredientId == ingRemoveId).Id;

        // 1. Verify nhánh ADD
        _recipeRepoMock.Verify(x => x.AddAsync(It.Is<InventoryService.Domain.Entities.Recipe>(r => r.IngredientId == ingAddId)), Times.Once);

        // 2. Verify nhánh UPDATE
        _recipeRepoMock.Verify(x => x.UpdateAsync(It.Is<InventoryService.Domain.Entities.Recipe>(r => r.IngredientId == ingUpdateId && r.Quantity == 20)), Times.Once);

        // 3. Verify nhánh DELETE
        _recipeRepoMock.Verify(x => x.DeleteAsync(removedRecipeId), Times.Once);

        // Verify Commit
        _transactionMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRollback_WhenErrorOccurs()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var request = new MutationRecipeReqDto { VariantId = variantId, Ingredients = new List<RecipeIngredientRequest>() };
        var existingRecipes = new List<InventoryService.Domain.Entities.Recipe> { new() { IngredientId = Guid.NewGuid(), Id = Guid.NewGuid() } };

        _productClientMock.Setup(x => x.GetVariantById(variantId)).ReturnsAsync(new ProductVariantRespone());
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdAsync(variantId, It.IsAny<CancellationToken>())).ReturnsAsync(existingRecipes);

        // Ép lỗi khi xóa để phủ nhánh catch và Rollback
        _recipeRepoMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("Fail"));

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new MutationRecipeCommand(request), CancellationToken.None))
            .Should().ThrowAsync<Exception>();

        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}