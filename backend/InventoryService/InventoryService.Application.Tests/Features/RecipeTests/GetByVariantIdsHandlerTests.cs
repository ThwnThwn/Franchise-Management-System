using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Dtos.Recipe;
using InventoryService.Application.Features.Recipe.Queries.GetByVariantIds;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryService.Application.Tests.Features.Recipe;

public class GetByVariantIdsHandlerTests
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetByVariantIdsHandler _handler;

    public GetByVariantIdsHandlerTests()
    {
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetByVariantIdsHandler(_recipeRepoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnList_WhenRecipesExist()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var variantIds = new List<Guid> { variantId };

        // Khởi tạo query đảm bảo data không bị null
        var query = new GetByVariantIdsQuery(variantIds);

        var recipesData = new List<InventoryService.Domain.Entities.Recipe>
        {
            new() {
                VariantId = variantId,
                IngredientId = Guid.NewGuid(),
                Quantity = 5
            }
        };

        // 1. Mock Repository: Trả về list có 1 phần tử
        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipesData);

        // 2. Mock Mapper: Quan trọng nhất - phải trả về Object cụ thể, không được để mặc định (null)
        _mapperMock.Setup(m => m.Map<RecipeViewRes>(It.IsAny<InventoryService.Domain.Entities.Recipe>()))
            .Returns(new RecipeViewRes
            {
                VariantId = variantId,
                Quantity = 5
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1, "Bởi vì Repository đã trả về 1 Recipe và Mapper đã map thành công");
        result[0].VariantId.Should().Be(variantId);

        _recipeRepoMock.Verify(x => x.GetRecipesByVariantIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<RecipeViewRes>(It.IsAny<InventoryService.Domain.Entities.Recipe>()), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldReturnEmptyList_WhenNoRecipesFound(bool isNull)
    {
        // Arrange
        var query = new GetByVariantIdsQuery(new List<Guid> { Guid.NewGuid() });

        _recipeRepoMock.Setup(x => x.GetRecipesByVariantIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(isNull ? null : new List<InventoryService.Domain.Entities.Recipe>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}