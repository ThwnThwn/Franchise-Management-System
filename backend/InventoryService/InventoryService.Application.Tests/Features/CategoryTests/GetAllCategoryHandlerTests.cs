using FluentAssertions;
using InventoryService.Application.Features.Category.Queries.GetAllCategory;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using Moq;

namespace InventoryService.Application.Tests.Features.Category;

public class GetAllCategoryHandlerTests
{
    private readonly Mock<IIngredientCategoryRepository> _categoryRepositoryMock;
    private readonly GetAllCategoryHandler _handler;

    public GetAllCategoryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IIngredientCategoryRepository>();
        _handler = new GetAllCategoryHandler(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnListOfCategoryViewRes_WhenCategoriesExist()
    {
        // Arrange
        var categories = new List<IngredientCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "Cà phê", Description = "Các loại hạt cà phê" },
            new() { Id = Guid.NewGuid(), Name = "Sữa", Description = "Sữa tươi, sữa đặc" }
        };

        _categoryRepositoryMock.Setup(repo => repo.GetAllAsync(null, null))
            .ReturnsAsync(categories);

        var query = new GetAllCategoryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Cà phê");
        result[1].Name.Should().Be("Sữa");

        // Kiểm tra xem Id có được map đúng không
        result[0].Id.Should().Be(categories[0].Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCategoriesExist()
    {
        // Arrange
        _categoryRepositoryMock.Setup(repo => repo.GetAllAsync(null, null))
            .ReturnsAsync(new List<IngredientCategory>());

        var query = new GetAllCategoryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}