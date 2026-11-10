using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Dtos.Ingredient;
using InventoryService.Application.Features.Ingrdient.Queries.GetIngredientById;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

// Tuân thủ đúng cấu trúc Namespace Duy yêu cầu
namespace InventoryService.Application.Tests.Features.IngredientTests;

public class GetIngredientByIdHandlerTests
{
    private readonly Mock<IIngredientRepository> _ingredientRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetIngredientByIdHandler _handler;

    public GetIngredientByIdHandlerTests()
    {
        _ingredientRepoMock = new Mock<IIngredientRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetIngredientByIdHandler(_ingredientRepoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnIngredient_WhenIdExists()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var query = new GetIngredientByIdQuery(ingredientId);

        var ingredient = new Domain.Entities.Ingredient
        {
            Id = ingredientId,
            Name = "Hạt cà phê",
            BaseUnit = "Gram",
            CategoryId = Guid.NewGuid()
        };

        var expectedRes = new IngredientViewRes
        {
            Id = ingredientId,
            Name = "Hạt cà phê",
            BaseUnit = "Gram"
        };

        _ingredientRepoMock.Setup(x => x.GetByIdAsync(ingredientId))
            .ReturnsAsync(ingredient);

        _mapperMock.Setup(m => m.Map<IngredientViewRes>(ingredient))
            .Returns(expectedRes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(ingredientId);
        result.Name.Should().Be("Hạt cà phê");

        _ingredientRepoMock.Verify(x => x.GetByIdAsync(ingredientId), Times.Once);
        _mapperMock.Verify(m => m.Map<IngredientViewRes>(ingredient), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var query = new GetIngredientByIdQuery(ingredientId);

        // Giả lập Repository trả về null (Branch: ingredient is null)
        _ingredientRepoMock.Setup(x => x.GetByIdAsync(ingredientId))
            .ReturnsAsync((Domain.Entities.Ingredient?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _ingredientRepoMock.Verify(x => x.GetByIdAsync(ingredientId), Times.Once);
        // Đảm bảo không gọi Mapping nếu dữ liệu null (Phủ nhánh return null)
        _mapperMock.Verify(m => m.Map<IngredientViewRes>(It.IsAny<Domain.Entities.Ingredient>()), Times.Never);
    }
}