using AutoMapper;
using FluentAssertions;
using Moq;
using ProductService.Application.Features.Categories.Queries.GetList;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Tests.Features.Categories;

public class GetListHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCategoriesHandler _handler;

    public GetListHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetCategoriesHandler(_categoryRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCategoriesExist()
    {
        // Arrange
        var query = new GetCategoriesQuery();
        var emptyCategoryList = new List<Category>();
        var emptyDtoList = new List<GetCategoryListItemDto>();

        _categoryRepositoryMock
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyCategoryList);

        _mapperMock
            .Setup(m => m.Map<List<GetCategoryListItemDto>>(emptyCategoryList))
            .Returns(emptyDtoList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Categories.Should().NotBeNull();
        result.Value.Categories.Should().BeEmpty();

        _categoryRepositoryMock.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnListOfCategories_WhenCategoriesExist()
    {
        // Arrange
        var query = new GetCategoriesQuery();

        var category1 = new Category { Id = Guid.NewGuid(), Name = "Category 1", IsActive = true };
        var category2 = new Category { Id = Guid.NewGuid(), Name = "Category 2", IsActive = false };
        var categoryList = new List<Category> { category1, category2 };

        var dto1 = new GetCategoryListItemDto { Id = category1.Id, Name = category1.Name, IsActive = true };
        var dto2 = new GetCategoryListItemDto { Id = category2.Id, Name = category2.Name, IsActive = false };
        var dtoList = new List<GetCategoryListItemDto> { dto1, dto2 };

        _categoryRepositoryMock
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryList);

        _mapperMock
            .Setup(m => m.Map<List<GetCategoryListItemDto>>(categoryList))
            .Returns(dtoList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Categories.Should().NotBeNull();
        result.Value.Categories.Should().HaveCount(2);

        // Kiểm tra chi tiết phần tử đầu tiên xem map có chuẩn không
        result.Value.Categories.First().Id.Should().Be(category1.Id);
        result.Value.Categories.First().Name.Should().Be(category1.Name);

        _categoryRepositoryMock.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}