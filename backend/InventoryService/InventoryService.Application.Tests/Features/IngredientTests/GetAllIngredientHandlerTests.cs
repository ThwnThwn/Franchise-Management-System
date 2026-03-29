using AutoMapper;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using InventoryService.Application.Dtos.Ingredient;
using InventoryService.Application.Features.Ingrdient.Queries.GetAllIngredient;
using InventoryService.Application.Persistences.Repositories;
using Moq;

namespace InventoryService.Application.Tests.Features.IngredientTests;

public class GetAllIngredientHandlerTests
{
    private readonly Mock<IIngredientRepository> _ingredientRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllIngredientHandler _handler;

    public GetAllIngredientHandlerTests()
    {
        _ingredientRepoMock = new Mock<IIngredientRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetAllIngredientHandler(_ingredientRepoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WhenIngredientsExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetAllIngredientQuery
        {
            CategoryId = categoryId,
            Page = 1,
            PageSize = 10
        };

        var ingredients = new List<Domain.Entities.Ingredient>
        {
            new() { Id = Guid.NewGuid(), Name = "Sữa tươi", BaseUnit = "Litre", CategoryId = categoryId },
            new() { Id = Guid.NewGuid(), Name = "Đường", BaseUnit = "Gram", CategoryId = categoryId }
        };

        // Mock kết quả phân trang trả về từ Repository
        var pagedResultFromRepo = new PagedResult<Domain.Entities.Ingredient>(ingredients, 2, 1, 10);

        _ingredientRepoMock.Setup(x => x.GetPagedIngredientsAsync(
                query.CategoryId,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResultFromRepo);

        // Mock Mapper trả về DTO cụ thể để tránh null trong Select
        _mapperMock.Setup(m => m.Map<IngredientViewRes>(It.IsAny<Domain.Entities.Ingredient>()))
            .Returns((Domain.Entities.Ingredient src) => new IngredientViewRes
            {
                Id = src.Id,
                Name = src.Name,
                BaseUnit = src.BaseUnit,
                CategoryId = src.CategoryId
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.TotalPages.Should().Be(1);

        _ingredientRepoMock.Verify(x => x.GetPagedIngredientsAsync(categoryId, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<IngredientViewRes>(It.IsAny<Domain.Entities.Ingredient>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPagedResult_WhenNoIngredientsFound()
    {
        // Arrange
        var query = new GetAllIngredientQuery { Page = 1, PageSize = 10 };

        // Trả về PagedResult với danh sách rỗng
        var emptyPagedResult = new PagedResult<Domain.Entities.Ingredient>(new List<Domain.Entities.Ingredient>(), 0, 1, 10);

        _ingredientRepoMock.Setup(x => x.GetPagedIngredientsAsync(
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);

        _mapperMock.Verify(m => m.Map<IngredientViewRes>(It.IsAny<Domain.Entities.Ingredient>()), Times.Never);
    }
}